# MongoDB 操作與專案執行指南

## 1. 專案執行的前置準備

若要執行專案測試，請依照以下步驟準備環境：

### 1.1 安裝必要的軟體與工具

- [安裝 .NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- [安裝 Docker Desktop](https://www.docker.com/products/docker-desktop)
- 拉取並運行 MongoDB Docker 映像檔（image）

### 1.2 啟動 MongoDB Docker 容器

使用以下命令啟動 MongoDB 容器，並將預設的 27017 埠號（port）映射到主機的 27017 埠號：

```bash
docker run -d -p 27017:27017 --name mongodb mongo
```

## 2. mongo db操作相關文件 放在 Doc中

## 3 如果懶得執行只想看程式寫法也ok，這邊提供三種mongo操作方式 

物件操作方式(Demos/OrmQuery.cs)<br>
BSON操作方式 (Demos/AggregateQuery.cs)<br>
dynamic 操作方式(Program.cs => 其他查詢範例)<br>