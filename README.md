🚂 Locomotiva WPF 

Aplicação WPF em **C# e .NET 8** que simula uma **locomotiva clássica** em movimento, com:

- Rodas animadas e sincronizadas com as bielas
- Trilhos e dormentes renderizados dinamicamente
- Emissão de fumaça pela chaminé
- Layout responsivo (usa `Viewbox`)
- Código comentado e organizado

## ▶️ Como Executar

### Requisitos
- Windows 10/11  
- .NET 8 Desktop Runtime (se usar a versão **não auto-contida**)  

### Rodando a partir do código
1. Clone o repositório:
   ```bash
   git clone https://github.com/mhbarcellos/locomotiva-wpf.git
   ```
2. Abra o projeto no **Visual Studio 2022+**  
3. Compile e rode com **Ctrl+F5**

### Baixando o App pronto
Na aba [**Releases**](https://github.com/mhbarcellos/locomotiva-wpf/releases), faça o download do arquivo `.zip`, extraia e execute:
```
TrabalhoC1.exe
```

Se preferir, use a versão **self-contained**, que não exige instalar .NET.

---

## 📦 Publicação

Para gerar um executável distribuível:

```powershell
dotnet publish -c Release -r win-x64 `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:TrimUnusedDependencies=false `
  -o publish\win-x64
```

O resultado ficará em `publish\win-x64\TrabalhoC1.exe`.

---

## 🛠️ Tecnologias

- C#
- WPF (Windows Presentation Foundation)
- .NET 8.0

---
