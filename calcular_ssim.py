from PIL import Image
import numpy as np

def ssim(img1, img2):
    img1 = np.array(Image.open(img1).resize((512,512)).convert('RGB'), dtype=np.float64)
    img2 = np.array(Image.open(img2).resize((512,512)).convert('RGB'), dtype=np.float64)
    mu1, mu2 = img1.mean(), img2.mean()
    s1, s2 = img1.std(), img2.std()
    cov = ((img1 - mu1) * (img2 - mu2)).mean()
    C1, C2 = 6.5025, 58.5225
    ssim_val = (2*mu1*mu2 + C1) * (2*cov + C2) / ((mu1**2 + mu2**2 + C1) * (s1**2 + s2**2 + C2))
    return round(ssim_val, 4)

pares = [
    ("textura_pedra.png", "real_pedra.png", "Pedra"),
    ("textura_madeira.png", "real_madeira.png", "Madeira"),
    ("textura_relva.png", "real_relva.png", "Relva"),
]

print("="*45)
print(f"{'Textura':<12} {'SSIM':>8}  {'Qualidade'}")
print("="*45)
for gerada, real, nome in pares:
    val = ssim(gerada, real)
    q = "Excelente" if val > 0.8 else "Boa" if val > 0.6 else "Moderada" if val > 0.4 else "Baixa"
    print(f"{nome:<12} {val:>8}  {q}")
print("="*45)