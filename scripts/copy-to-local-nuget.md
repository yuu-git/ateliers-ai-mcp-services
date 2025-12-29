## 開発者向け：ローカルNuGetフィード設定

### 初回セットアップ

#### 1. ローカルNuGetフィードを登録（一度だけ）
```bash
dotnet nuget add source "C:\LocalNuGet" --name "LocalDev"
```

登録確認：
```bash
dotnet nuget list source
```

削除する場合：
```bash
dotnet nuget remove source "LocalDev"
```

#### 2. カスタムパスを使う場合（オプション）

デフォルトでは `C:\LocalNuGet` が使用されます。別のパスを使いたい場合は環境変数を設定してください。

**永続的な設定（推奨）：**

PowerShell（管理者権限不要）：
```powershell
[System.Environment]::SetEnvironmentVariable(
    'ATELIERS_LOCAL_NUGET_PATH', 
    'D:\MyNuGet',  # お好みのパスに変更
    'User'
)
```

または GUI で設定：
1. `Windows + R` → `sysdm.cpl` → Enter
2. 「詳細設定」タブ → 「環境変数」
3. ユーザー環境変数の「新規」をクリック
   - 変数名: `ATELIERS_LOCAL_NUGET_PATH`
   - 変数値: `D:\MyNuGet`（お好みのパス）

**一時的な設定（現在のセッションのみ）：**
```powershell
$env:ATELIERS_LOCAL_NUGET_PATH = "D:\MyNuGet"
```

### 使用方法

1. プロジェクトをビルド
```bash
   dotnet build -c Release
```

2. スクリプトを実行してローカルフィードにコピー
```powershell
   .\scripts\copy-to-local-nuget.ps1
```

3. Visual Studio でパッケージ参照を更新

### トラブルシューティング

- **古いバージョンがキャッシュされている場合**
```bash
  dotnet nuget locals all --clear
```

- **特定のパスを一時的に使いたい場合**
```powershell
  .\scripts\copy-to-local-nuget.ps1 -LocalFeedPath "D:\TempNuGet"
```