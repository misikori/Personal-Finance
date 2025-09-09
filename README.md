# Personal-Finance

## Dev setup

1. Install pre-commit
    ```
   pip install pre-commit
    ```
2. Install .NET SDK (>=8.0.x)
3. Install pre-commit hooks
    ```
    pre-commit install --hook-type pre-commit --hook-type commit-msg
    ```
4. Commit code normally. Hooks will:
    
    - Check commit messages (gitlint)
    - Auto-format C# files (dotnet format)

