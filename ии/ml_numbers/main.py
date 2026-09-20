import torch
import torch.nn as nn
from torchvision import transforms
from pathlib import Path
from PIL import Image
import json

class SimpleNet(nn.Module):
    def __init__(self):
        super().__init__()
        self.fc1 = nn.Linear(64 * 64, 128)
        self.fc2 = nn.Linear(128, 10)

    def forward(self, x):
        x = x.view(-1, 64 * 64)
        x = torch.relu(self.fc1(x))
        return self.fc2(x)


def load_custom_dataset(data_dir):
    """Загружает изображения из папки data/custom/."""
    data_dir = Path(data_dir)
    images = []
    labels = []

    for filepath in sorted(data_dir.glob("*.png")):
        filename = filepath.stem  # e.g. "3_0002"
        digit = int(filename.split("_")[0])

        img = Image.open(filepath).convert("L") # цвета в числа
        img = img.resize((64, 64))

        images.append(img)
        labels.append(digit)

    if not images:
        raise FileNotFoundError(
            f"В папке {data_dir} нет изображений. "
            f"Положите .png файлы в data/custom/"
        )

    return images, labels


def train_model(epochs=10):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
    model = SimpleNet().to(device)

    # Загружаем свой датасет
    images, labels = load_custom_dataset("./data/custom")
    print(f"Найдено {len(images)} примеров")

    # Считаем статистику для нормализации по своему датасету
    tensors = [transforms.ToTensor()(img) for img in images] # 0-255 to 0.0-1.0
    mean = float(torch.stack(tensors).mean())  # среднее
    std = float(torch.stack(tensors).std()) # стандартное отклонение
    print(f"Нормализация: mean={mean:.4f}, std={std:.4f}")

    # Сохраняем статистику для predict()
    stats = {"mean": mean, "std": std}
    with open("dataset_stats.json", "w") as f:
        json.dump(stats, f)

    transform = transforms.Compose([
        transforms.ToTensor(),
        transforms.Normalize((mean,), (std,)),
    ])

    # Создаём датасет из набора картинок
    dataset = torch.utils.data.TensorDataset(
        torch.stack([transform(img) for img in images]),
        torch.tensor(labels, dtype=torch.long),
    )
    # Наборы данных перемешанными пачками
    loader = torch.utils.data.DataLoader(dataset, batch_size=32, shuffle=True)

    optimizer = torch.optim.Adam(model.parameters(), lr=0.001)
    criterion = nn.CrossEntropyLoss()

    for epoch in range(epochs):
        model.train()
        total_loss = 0
        for imgs, lbls in loader:
            imgs, lbls = imgs.to(device), lbls.to(device)
            optimizer.zero_grad()
            loss = criterion(model(imgs), lbls)
            loss.backward()
            optimizer.step()
            total_loss += loss.item()

        model.eval()
        correct = total = 0
        with torch.no_grad():
            for imgs, lbls in loader:
                imgs, lbls = imgs.to(device), lbls.to(device)
                pred = model(imgs).argmax(1)
                correct += (pred == lbls).sum().item()
                total += lbls.size(0)

        acc = 100 * correct / total
        print(
            f"Epoch {epoch+1}/{epochs} — "
            f"Loss: {total_loss/len(loader):.4f}, "
            f"Accuracy: {acc:.1f}%"
        )

    torch.save(model.state_dict(), "model.pth")
    print("model.pth сохранён")


def predict(image_path):
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    # Загружаем статистику датасета
    with open("dataset_stats.json") as f:
        stats = json.load(f)
    mean, std = stats["mean"], stats["std"]

    model = SimpleNet().to(device)
    model.load_state_dict(torch.load("model.pth", weights_only=True))
    model.eval()

    # Открываем картинку
    img = Image.open(image_path).convert("L").resize((64, 64))
    tensor = transforms.ToTensor()(img)
    tensor = transforms.Normalize((mean,), (std,))(tensor)
    tensor = tensor.unsqueeze(0).to(device)

    with torch.no_grad():
        logits = model(tensor)
        probs = torch.softmax(logits, dim=1)
        pred = probs.argmax(1).item()
        confidence = probs.max().item()

    print(f"Распознана цифра: {pred}")
    print(f"Вероятность: {confidence:.2%}")
    print("\nВероятности по классам:")
    for digit, prob in enumerate(probs[0]):
        bar = "█" * int(prob.item() * 30)
        print(f"  {digit}: {prob.item():.4f} {bar}")


if __name__ == "__main__":
    import sys

    if len(sys.argv) > 1 and sys.argv[1] == "train":
        epochs = int(sys.argv[2]) if len(sys.argv) > 2 else 10
        train_model(epochs)
    elif len(sys.argv) > 1:
        predict(sys.argv[1])
    else:
        print("python main.py train [epochs]   — обучить модель на data/custom/")
        print("python main.py <image.png>      — распознать цифру")
