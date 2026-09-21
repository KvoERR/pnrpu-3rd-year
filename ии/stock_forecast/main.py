import pandas as pd
import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import TensorDataset, DataLoader
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
import json
import sys
from pathlib import Path


# ──────────────────────────── helpers ────────────────────────────

def parse_number(x):
    """'280,50' → 280.5, '11,21K' → 11210"""
    if pd.isna(x):
        return np.nan
    x = str(x).strip()
    mult = 1
    if 'K' in x:
        mult = 1000
        x = x.replace('K', '')
    elif 'M' in x:
        mult = 1_000_000
        x = x.replace('M', '')
    x = x.replace('%', '').replace(',', '.')
    try:
        return float(x) * mult
    except ValueError:
        return np.nan


def load_coffee(path):
    """Загружает CSV и парсит числа/даты."""
    df = pd.read_csv(path, sep=',', encoding='utf-8-sig')
    df.columns = ['date', 'price', 'open', 'high', 'low', 'volume', 'change_pct']

    for col in ['price', 'open', 'high', 'low', 'volume', 'change_pct']:
        df[col] = df[col].apply(parse_number)

    df['date'] = pd.to_datetime(df['date'], format='%d.%m.%Y')
    df = df.sort_values('date').reset_index(drop=True)
    return df


def make_lag_dataset(df, lags=(1, 2, 3, 5, 10, 22)):
    """
    Для каждой даты собирает лаги цены и саму цену (target).
    
    Возвращает DataFrame:
        date | price | lag_1 | lag_2 | ... | lag_N
    """
    out = pd.DataFrame()
    out['date']  = df['date']
    out['price'] = df['price']            # target

    for lag in lags:
        out[f'lag_{lag}'] = df['price'].shift(lag)

    out = out.dropna().reset_index(drop=True)
    return out


# ──────────────────────────── MLP model ────────────────────────────

class SimpleMLP(nn.Module):
    """Простая полносвязная сеть для прогнозирования цены."""

    def __init__(self, input_dim=6):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_dim, 32),
            nn.ReLU(),
            nn.Linear(32, 1),
        )

    def forward(self, x):
        return self.net(x)


# ──────────────────────────── metrics ────────────────────────────

def mae(y_true, y_pred):
    return np.mean(np.abs(y_true - y_pred))


def rmse(y_true, y_pred):
    return np.sqrt(np.mean((y_true - y_pred) ** 2))


def mape(y_true, y_pred):
    mask = y_true != 0
    return np.mean(np.abs((y_true[mask] - y_pred[mask]) / y_true[mask])) * 100


# ──────────────────────────── train ────────────────────────────

def train_model(epochs=50, lr=0.001, batch_size=32, lags=(1, 2, 3, 5, 10, 22)):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    print(f"Device: {device}")

    # 1. Загрузка данных
    df = load_coffee('data.csv')
    print(f"Загружено: {len(df)} строк, период: {df['date'].min().date()} — {df['date'].max().date()}")

    lag_df = make_lag_dataset(df, lags=lags)
    print(f"Датасет с лагами: {len(lag_df)} строк")

    feature_cols = [f'lag_{l}' for l in lags]
    X = lag_df[feature_cols].values.astype(np.float32)
    y = lag_df['price'].values.astype(np.float32)

    # 2. Разделение: 70% train, 15% val, 15% test
    X_train_val, X_test, y_train_val, y_test, dates_train_val, dates_test = train_test_split(
        X, y, lag_df['date'].values, test_size=0.15, random_state=42
    )
    X_train, X_val, y_train, y_val, dates_train, dates_val = train_test_split(
        X_train_val, y_train_val, dates_train_val, test_size=0.176, random_state=42  # 0.176 * 0.85 ≈ 0.15
    )

    print(f"\nTrain: {len(X_train)}, Val: {len(X_val)}, Test: {len(X_test)}")

    # 3. Нормализация только признаков
    scaler_X = StandardScaler()
    scaler_y = StandardScaler()

    X_train = scaler_X.fit_transform(X_train)
    X_val   = scaler_X.transform(X_val)
    X_test  = scaler_X.transform(X_test)

    y_train = scaler_y.fit_transform(y_train.reshape(-1, 1)).ravel()
    y_val   = scaler_y.transform(y_val.reshape(-1, 1)).ravel()

    # Сохраняем статистику
    stats = {
        'scaler_X_mean': scaler_X.mean_.tolist(),
        'scaler_X_std':  scaler_X.scale_.tolist(),
        'scaler_y_mean': float(scaler_y.mean_[0]),
        'scaler_y_std':  float(scaler_y.scale_[0]),
        'feature_cols': feature_cols,
    }
    with open('norm_stats.json', 'w') as f:
        json.dump(stats, f, indent=2)
    print("norm_stats.json сохранён")

    # 4. DataLoader
    train_ds = TensorDataset(
        torch.tensor(X_train), torch.tensor(y_train)
    )
    val_ds = TensorDataset(
        torch.tensor(X_val), torch.tensor(y_val)
    )
    train_loader = DataLoader(train_ds, batch_size=batch_size, shuffle=True)
    val_loader   = DataLoader(val_ds, batch_size=batch_size, shuffle=False)

    # 5. Модель
    model = SimpleMLP(input_dim=len(feature_cols)).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=lr)
    criterion = nn.MSELoss()

    # 6. Обучение
    best_val_loss = float('inf')
    best_state = None

    for epoch in range(epochs):
        # train
        model.train()
        train_loss = 0
        for xb, yb in train_loader:
            xb, yb = xb.to(device), yb.to(device)
            optimizer.zero_grad()
            loss = criterion(model(xb), yb)
            loss.backward()
            optimizer.step()
            train_loss += loss.item()

        # val
        model.eval()
        val_loss = 0
        with torch.no_grad():
            for xb, yb in val_loader:
                xb, yb = xb.to(device), yb.to(device)
                val_loss += criterion(model(xb), yb).item()

        avg_train = train_loss / len(train_loader)
        avg_val   = val_loss / len(val_loader)

        if (epoch + 1) % 10 == 0 or epoch == 0:
            print(f"Epoch {epoch+1:3d}/{epochs} — Train loss: {avg_train:.6f}, Val loss: {avg_val:.6f}")

        # сохраняем лучшую модель
        if avg_val < best_val_loss:
            best_val_loss = avg_val
            best_state = {k: v.clone() for k, v in model.state_dict().items()}

    # Восстанавливаем лучшую модель
    model.load_state_dict(best_state)

    # 7. Сохранение
    torch.save(model.state_dict(), 'coffee_model.pth')
    print("coffee_model.pth сохранён")

    # 8. Оценка на test set
    X_test_t = torch.tensor(X_test).to(device)
    y_test_t = torch.tensor(y_test).to(device)

    model.eval()
    with torch.no_grad():
        y_pred_scaled = model(X_test_t).cpu().numpy().ravel()

    # Обратная нормализация
    y_pred = scaler_y.inverse_transform(y_pred_scaled.reshape(-1, 1)).ravel()
    y_test_orig = scaler_y.inverse_transform(y_test.reshape(-1, 1)).ravel()

    print(f"\n=== Метрики на Test set ({len(X_test)} строк) ===")
    print(f"  MAE:  {mae(y_test_orig, y_pred):.2f}")
    print(f"  RMSE: {rmse(y_test_orig, y_pred):.2f}")
    print(f"  MAPE: {mape(y_test_orig, y_pred):.2f}%")

    # 9. Визуализация
    try:
        import matplotlib
        matplotlib.use('Agg')
        import matplotlib.pyplot as plt

        dates_test = dates_test[:len(X_test)]

        fig, axes = plt.subplots(1, 2, figsize=(14, 5))

        # График предсказаний
        ax = axes[0]
        ax.plot(range(len(y_test_orig)), y_test_orig, label='Реальная цена', color='blue', linewidth=1.5)
        ax.plot(range(len(y_pred)), y_pred, label='Предсказание', color='red', linewidth=1.5, alpha=0.7)
        ax.set_title('Реальная vs предсказанная цена', fontsize=13)
        ax.set_xlabel('Строка в test-сете')
        ax.set_ylabel('Цена')
        ax.legend()
        ax.grid(True, alpha=0.3)

        # Распределение ошибок
        ax = axes[1]
        errors = y_test_orig - y_pred
        ax.hist(errors, bins=40, color='steelblue', edgecolor='black', alpha=0.8)
        ax.set_title('Распределение ошибок', fontsize=13)
        ax.set_xlabel('Ошибка (реальная − предсказанная)')
        ax.set_ylabel('Количество')
        ax.axvline(x=0, color='red', linestyle='--', linewidth=1.5)
        ax.grid(True, alpha=0.3)

        plt.tight_layout()
        plt.savefig('prediction_plot.png', dpi=150)
        print("prediction_plot.png сохранён")
        plt.close()
    except ImportError:
        print("\nmatplotlib не установлен — график не строим")

    return model, stats


# ──────────────────────────── predict ────────────────────────────

def predict(lag_values):
    """Прогноз по значениям лагов цены."""
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    with open('norm_stats.json') as f:
        stats = json.load(f)

    feature_cols = stats['feature_cols']
    input_dim = len(feature_cols)

    model = SimpleMLP(input_dim=input_dim).to(device)
    model.load_state_dict(torch.load('coffee_model.pth', weights_only=True, map_location=device))
    model.eval()

    scaler_X = StandardScaler()
    scaler_X.mean_ = np.array(stats['scaler_X_mean'])
    scaler_X.scale_ = np.array(stats['scaler_X_std'])
    scaler_y = StandardScaler()
    scaler_y.mean_ = np.array([stats['scaler_y_mean']])
    scaler_y.scale_ = np.array([stats['scaler_y_std']])

    X = np.array([lag_values], dtype=np.float32)
    X = scaler_X.transform(X)

    with torch.no_grad():
        X_t = torch.tensor(X).to(device)
        y_pred_scaled = model(X_t).cpu().numpy().ravel()

    y_pred = scaler_y.inverse_transform(y_pred_scaled.reshape(-1, 1)).ravel()[0]

    print(f"\nЛаги: {lag_values}")
    print(f"Предсказанная цена: {y_pred:.2f}")


# ──────────────────────────── CLI ────────────────────────────

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Использование:")
        print("  python main.py train [epochs]       — обучить модель")
        print("  python main.py predict l1 l2 l3 l5 l10 l22  — прогноз по лагам")
        print()
        print("Примеры:")
        print("  python main.py train 100")
        print("  python main.py predict 246 251 252 251 241 244")
    elif sys.argv[1] == 'train':
        epochs = int(sys.argv[2]) if len(sys.argv) > 2 else 50
        train_model(epochs=epochs)
    elif sys.argv[1] == 'predict':
        if len(sys.argv) < 8:
            print("Укажи 6 лагов: python main.py predict l1 l2 l3 l5 l10 l22")
        else:
            lags = [float(x) for x in sys.argv[2:8]]
            predict(lags)
    else:
        print(f"Неизвестная команда: {sys.argv[1]}")
