import pandas as pd
import numpy as np


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
    df = pd.read_csv(path, sep=';', encoding='utf-8-sig')
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

    # Убираем строки, где лаги ещё не набрались (первые max(lags) строк)
    out = out.dropna().reset_index(drop=True)
    return out


if __name__ == "__main__":
    # 1. Загрузка
    df = load_coffee('data.csv')
    print(f"Загружено: {len(df)} строк")
    print(f"Период: {df['date'].min().date()} — {df['date'].max().date()}")

    # 2. Датасет с лагами
    lags = (1, 2, 3, 5, 10, 22)
    lag_df = make_lag_dataset(df, lags=lags)

    print(f"\nДатасет с лагами: {len(lag_df)} строк")
    print(f"Колонки: {list(lag_df.columns)}")
    print("\nПервые 5 строк:")
    print(lag_df.head())

    print("\nПоследние 5 строк:")
    print(lag_df.tail())

    # 3. Сохраняем
    lag_df.to_csv('coffee_lags.csv', index=False)
    print("\nСохранено в coffee_lags.csv")