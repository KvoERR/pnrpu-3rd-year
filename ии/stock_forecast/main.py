import pandas as pd
import numpy as np
import torch
import torch.nn as nn
from torch.utils.data import TensorDataset, DataLoader
from sklearn.preprocessing import StandardScaler
import json


def parse_number(x):
    if pd.isna(x):
        return np.nan
    x = str(x).strip()
    if x == '':
        return np.nan
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


def load_data(path):
    df = pd.read_csv(path, sep=';', encoding='utf-8-sig')
    df.columns = [c.strip() for c in df.columns]
    df['Дата'] = pd.to_datetime(df['Дата'], format='%d.%m.%Y')
    for col in ['Цена', 'Откр.', 'Макс.', 'Мин.', 'Объём']:
        df[col] = df[col].apply(parse_number)
    df = df.sort_values('Дата').reset_index(drop=True)
    df = df.dropna(subset=['Цена', 'Откр.', 'Макс.', 'Мин.'])
    df['volume_missing'] = df['Объём'].isna().astype(int)
    df['Объём'] = df['Объём'].fillna(df['Объём'].median())
    df['is_illiquid'] = (df['Объём'] < 0.1).astype(int)
    return df


LAGS = (1, 2, 3, 5, 10, 22)
MIN_HISTORY = 30


def add_price_features(p, i):
    f = {}
    for lag in LAGS:
        f[f'lag_{lag}'] = p[i - lag]
    f['ret_1']  = np.log(p[i - 1] / p[i - 2])
    f['ret_5']  = np.log(p[i - 1] / p[i - 6])
    f['ret_22'] = np.log(p[i - 1] / p[i - 23])
    f['ma_ratio'] = np.mean(p[i - 5:i]) / (np.mean(p[i - 22:i]) + 1e-9)
    f['vol_5']  = np.std(np.diff(np.log(p[i - 6:i])))
    f['vol_22'] = np.std(np.diff(np.log(p[i - 23:i])))
    return f


def add_ohlc_features(df, i):
    o = df['Откр.'].iloc[i]
    h = df['Макс.'].iloc[i]
    l = df['Мин.'].iloc[i]
    c = df['Цена'].iloc[i]
    prev_c = df['Цена'].iloc[i - 1]
    return {
        'hl_range':   (h - l) / (c + 1e-9),
        'body':       (c - o) / (c + 1e-9),
        'upper_wick': (h - max(o, c)) / (c + 1e-9),
        'lower_wick': (min(o, c) - l) / (c + 1e-9),
        'close_pos':  (c - l) / (h - l + 1e-9),
        'gap':        (o - prev_c) / (prev_c + 1e-9),
    }


def add_volume_features(df, i):
    v = df['Объём'].iloc[i]
    v_ma5  = df['Объём'].iloc[i - 5:i].mean()
    v_ma22 = df['Объём'].iloc[i - 22:i].mean()
    v_std  = df['Объём'].iloc[i - 22:i].std()
    return {
        'vol_ratio_5':  v / (v_ma5 + 1e-9),
        'vol_ratio_22': v / (v_ma22 + 1e-9),
        'vol_zscore':   (v - v_ma22) / (v_std + 1e-9),
        'volume_missing': float(df['volume_missing'].iloc[i]),
        'is_illiquid':    float(df['is_illiquid'].iloc[i]),
    }


def add_calendar_features(d):
    dow = d.weekday()
    dom = d.day
    doy = d.dayofyear
    return {
        'dow_sin': np.sin(2 * np.pi * dow / 7),
        'dow_cos': np.cos(2 * np.pi * dow / 7),
        'dom_sin': np.sin(2 * np.pi * dom / 31),
        'dom_cos': np.cos(2 * np.pi * dom / 31),
        'doy_sin': np.sin(2 * np.pi * doy / 365.25),
        'doy_cos': np.cos(2 * np.pi * doy / 365.25),
        'is_month_start': int(d.day <= 3),
        'is_month_end':   int(d.day >= 28),
    }


def add_autocorr_features(p, i):
    if i < 30:
        return {'ret_wow': 0.0, 'ret_mom_4w': 0.0, 'ac_1w': 0.0}
    ret_1  = np.log(p[i - 1] / p[i - 2])
    ret_1w = np.log(p[i - 8] / p[i - 9])
    return {
        'ret_wow':    np.log(p[i - 1] / p[i - 8]),
        'ret_mom_4w': np.log(p[i - 1] / p[i - 29]),
        'ac_1w':      ret_1 * ret_1w,
    }


def make_features(df, i):
    p = df['Цена'].values
    d = df['Дата'].iloc[i]
    f = {}
    f.update(add_price_features(p, i))
    f.update(add_ohlc_features(df, i))
    f.update(add_volume_features(df, i))
    f.update(add_calendar_features(d))
    f.update(add_autocorr_features(p, i))
    return f


def build_dataset(df):
    rows, ys = [], []
    for i in range(MIN_HISTORY, len(df) - 1):
        rows.append(make_features(df, i))
        ys.append(df['Цена'].iloc[i + 1])
    X_df = pd.DataFrame(rows).replace([np.inf, -np.inf], np.nan).fillna(0.0)
    X = X_df.values.astype(np.float32)
    y = np.array(ys, dtype=np.float32)
    return X, y, list(X_df.columns)


class SimpleMLP(nn.Module):
    def __init__(self, input_dim):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_dim, 64),
            nn.ReLU(),
            nn.Dropout(0.1),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.Linear(32, 1),
        )

    def forward(self, x):
        return self.net(x)


def train_model(df, epochs=200, lr=1e-3, batch_size=32):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    X, y, feature_cols = build_dataset(df)

    split = int(len(X) * 0.8)
    X_train, X_val = X[:split], X[split:]
    y_train, y_val = y[:split], y[split:]

    scaler_X = StandardScaler().fit(X_train)
    scaler_y = StandardScaler().fit(y_train.reshape(-1, 1))

    X_train_s = scaler_X.transform(X_train)
    X_val_s   = scaler_X.transform(X_val)
    y_train_s = scaler_y.transform(y_train.reshape(-1, 1))
    y_val_s   = scaler_y.transform(y_val.reshape(-1, 1))

    train_ds = TensorDataset(torch.tensor(X_train_s), torch.tensor(y_train_s))
    train_loader = DataLoader(train_ds, batch_size=batch_size, shuffle=True)

    val_X = torch.tensor(X_val_s).to(device)
    val_y = torch.tensor(y_val_s).to(device)

    model = SimpleMLP(input_dim=len(feature_cols)).to(device)
    optimizer = torch.optim.Adam(model.parameters(), lr=lr, weight_decay=1e-5)
    criterion = nn.MSELoss()

    best_val, best_state, patience, bad = float('inf'), None, 15, 0
    for epoch in range(epochs):
        model.train()
        for xb, yb in train_loader:
            xb, yb = xb.to(device), yb.to(device)
            optimizer.zero_grad()
            loss = criterion(model(xb), yb)
            loss.backward()
            optimizer.step()
        model.eval()
        with torch.no_grad():
            val_loss = criterion(model(val_X), val_y).item()
        if val_loss < best_val:
            best_val = val_loss
            best_state = {k: v.clone() for k, v in model.state_dict().items()}
            bad = 0
        else:
            bad += 1
        if bad >= patience:
            break

    model.load_state_dict(best_state)
    torch.save(model.state_dict(), 'coffee_model.pth')

    with open('norm_stats.json', 'w') as f:
        json.dump({
            'scaler_X_mean': scaler_X.mean_.tolist(),
            'scaler_X_std':  scaler_X.scale_.tolist(),
            'scaler_y_mean': float(scaler_y.mean_[0]),
            'scaler_y_std':  float(scaler_y.scale_[0]),
            'feature_cols':  feature_cols,
        }, f, indent=2)

    naive_pred = np.array([df['Цена'].iloc[i] for i in range(MIN_HISTORY, len(df) - 1)])
    mae_naive = np.mean(np.abs(y_val - naive_pred[split:]))

    model.eval()
    with torch.no_grad():
        pred = scaler_y.inverse_transform(model(val_X).cpu().numpy()).ravel()
    mae = np.mean(np.abs(y_val - pred))

    print(f"train={len(X_train)}, val={len(X_val)}")
    print(f"Model MAE: {mae:.4f}")
    print(f"Naive MAE: {mae_naive:.4f}")
    print(f"Улучшение: {(1 - mae / mae_naive) * 100:+.2f}%")


def load_model(device):
    with open('norm_stats.json') as f:
        stats = json.load(f)
    feature_cols = stats['feature_cols']
    model = SimpleMLP(input_dim=len(feature_cols)).to(device)
    model.load_state_dict(torch.load('coffee_model.pth',
                                     weights_only=True, map_location=device))
    model.eval()

    scaler_X = StandardScaler()
    scaler_X.mean_  = np.array(stats['scaler_X_mean'])
    scaler_X.scale_ = np.array(stats['scaler_X_std'])
    scaler_y = StandardScaler()
    scaler_y.mean_  = np.array([stats['scaler_y_mean']])
    scaler_y.scale_ = np.array([stats['scaler_y_std']])
    return model, scaler_X, scaler_y, feature_cols


def find_index_for_date(df, target_date):
    target_date = pd.to_datetime(target_date, format='%d.%m.%Y')
    matches = df.index[df['Дата'] == target_date].tolist()
    if not matches:
        return None
    return matches[0]


def predict(df, target_date=None):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    if target_date is not None:
        idx_target = find_index_for_date(df, target_date)
        if idx_target is None:
            print(f"Дата {target_date} не найдена")
            print(f"Диапазон: {df['Дата'].iloc[0].date()} → {df['Дата'].iloc[-1].date()}")
            return
        idx_feat = idx_target - 1
        feats = make_features(df, idx_feat)
        feat_date  = df['Дата'].iloc[idx_feat]
        feat_price = df['Цена'].iloc[idx_feat]
        true_price = df['Цена'].iloc[idx_target]
        true_date  = df['Дата'].iloc[idx_target]
        print(f"\nПризнаки на: {feat_date.date()} (цена {feat_price:.2f})")
        print(f"Предсказываем на: {true_date.date()}")
    else:
        idx_feat = len(df) - 1
        feats = make_features(df, idx_feat)
        feat_price = df['Цена'].iloc[idx_feat]
        true_price = None
        print(f"\nПоследняя дата: {df['Дата'].iloc[idx_feat].date()}, цена: {feat_price:.2f}")

    model, scaler_X, scaler_y, feature_cols = load_model(device)

    x = np.array([[feats[c] for c in feature_cols]], dtype=np.float32)
    x = scaler_X.transform(x)

    with torch.no_grad():
        y_s = model(torch.tensor(x).to(device)).cpu().numpy()
    y = scaler_y.inverse_transform(y_s).ravel()[0]

    delta = y - feat_price
    pct = delta / feat_price * 100
    print(f"Прогноз: {y:.2f}  (Δ {delta:+.2f}, {pct:+.2f}%)")

    if true_price is not None:
        err = y - true_price
        print(f"Реальная: {true_price:.2f}   Ошибка: {err:+.2f} ({err/true_price*100:+.2f}%)")


def backtest(df):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    X, y, feature_cols = build_dataset(df)
    model, scaler_X, scaler_y, _ = load_model(device)

    X_s = scaler_X.transform(X)
    with torch.no_grad():
        pred = scaler_y.inverse_transform(model(torch.tensor(X_s).to(device)).cpu().numpy()).ravel()
    naive = np.array([df['Цена'].iloc[i] for i in range(MIN_HISTORY, len(df) - 1)])

    mae   = np.mean(np.abs(y - pred))
    mae_n = np.mean(np.abs(y - naive))
    mape  = np.mean(np.abs((y - pred) / y)) * 100
    rmse  = np.sqrt(np.mean((y - pred) ** 2))

    print(f"\nПримеров: {len(y)}")
    print(f"Model  MAE: {mae:.4f}   RMSE: {rmse:.4f}   MAPE: {mape:.2f}%")
    print(f"Naive  MAE: {mae_n:.4f}")
    print(f"Улучшение: {(1 - mae / mae_n) * 100:+.2f}%")


if __name__ == "__main__":
    df0 = load_data('data0.csv')
    train_model(df0, epochs=200)

    df1 = load_data('data1.csv')
    predict(df1,'17.09.2026')
    backtest(df1)