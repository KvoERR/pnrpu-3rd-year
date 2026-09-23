import pandas as pd
import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import TensorDataset, DataLoader
from sklearn.preprocessing import StandardScaler
import json
import sys


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


def load_csv(path):
    """Загружает CSV, возвращает (dates, prices)."""
    df = pd.read_csv(path, sep=',', encoding='utf-8-sig')
    prices = [parse_number(x) for x in df.iloc[:, 1].tolist()]
    dates = pd.to_datetime(df.iloc[:, 0], format='%d.%m.%Y').tolist()
    return dates, prices


def make_lags(prices, lags=(1, 2, 3, 5, 10, 22)):
    """
    Из списка цен создаёт два списка:
      - lag_list: [[lag1, lag2, ...], [...], ...]
      - price_list: [price, price, ...]
    """
    max_lag = max(lags)
    lag_list = []
    price_list = []

    for i in range(max_lag, len(prices)):
        lag_row = [prices[i - lag] for lag in lags]
        lag_list.append(lag_row)
        price_list.append(prices[i])

    return lag_list, price_list


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


# ──────────────────────────── train ────────────────────────────

def train_model(epochs=50, lr=0.001, batch_size=32, lags=(1, 2, 3, 5, 10, 22)):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    # 1. Загрузка данных
    dates, prices = load_csv('data.csv')
    lag_list, price_list = make_lags(prices, lags=lags)

    feature_cols = [f'lag_{l}' for l in lags]
    X = np.array(lag_list, dtype=np.float32)
    y = np.array(price_list, dtype=np.float32)

    # 2. Нормализация
    scaler_X = StandardScaler()
    scaler_y = StandardScaler()

    X = scaler_X.fit_transform(X)
    y = scaler_y.fit_transform(y.reshape(-1, 1)).ravel()

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

    # 3. DataLoader
    train_ds = TensorDataset(
        torch.tensor(X), torch.tensor(y)
    )
    train_loader = DataLoader(train_ds, batch_size=batch_size, shuffle=True)

    # 4. Модель
    model = SimpleMLP(input_dim=len(feature_cols)).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=lr)
    criterion = nn.MSELoss()

    # 5. Обучение
    best_loss = float('inf')
    best_state = None

    for epoch in range(epochs):
        model.train()
        epoch_loss = 0
        for xb, yb in train_loader:
            xb, yb = xb.to(device), yb.to(device)
            optimizer.zero_grad()
            loss = criterion(model(xb), yb)
            loss.backward()
            optimizer.step()
            epoch_loss += loss.item()

        avg_loss = epoch_loss / len(train_loader)
        if avg_loss < best_loss:
            best_loss = avg_loss
            best_state = {k: v.clone() for k, v in model.state_dict().items()}

    model.load_state_dict(best_state)
    torch.save(model.state_dict(), 'coffee_model.pth')

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
    print(make_lags())
    if len(sys.argv) < 2:
        print("Использование:")
        print("  python main.py train [epochs]       — обучить модель")
        print("  python main.py predict l1 l2 l3 l5 l10 l22  — прогноз по лагам")
        print("  python main.py get_lags             — показать текущие лаги")
        print()
        print("Примеры:")
        print("  python main.py train 100")
        print("  python main.py predict 246 251 252 251 241 244")
        print("  python main.py get_lags")
    elif sys.argv[1] == 'train':
        epochs = int(sys.argv[2]) if len(sys.argv) > 2 else 50
        train_model(epochs=epochs)
    elif sys.argv[1] == 'predict':
        if len(sys.argv) < 8:
            print("Укажи 6 лагов: python main.py predict l1 l2 l3 l5 l10 l22")
        else:
            lags = [float(x) for x in sys.argv[2:8]]
            predict(lags)
    elif sys.argv[1] == 'get_lags':
        lags,prices = make_lags()
        print(f"Текущие лаги: {lags}")
    else:
        print(f"Неизвестная команда: {sys.argv[1]}")
