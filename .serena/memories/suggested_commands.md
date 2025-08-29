# Suggested Commands for VRCQuestTools Development

## Windows System Commands
- `dir` - List directory contents (equivalent to `ls` on Unix)
- `cd` - Change directory
- `copy` - Copy files (equivalent to `cp` on Unix)
- `move` - Move/rename files (equivalent to `mv` on Unix)
- `del` - Delete files (equivalent to `rm` on Unix)
- `findstr` - Search text in files (equivalent to `grep` on Unix)
- `where` - Find executable files (equivalent to `which` on Unix)

## Git Commands
- `git status` - Check repository status
- `git add .` - Stage all changes
- `git commit -m "message"` - Commit changes
- `git push` - Push to remote repository
- `git pull` - Pull from remote repository
- `git branch` - List branches
- `git checkout branch-name` - Switch branches

## Development Commands

### Linting and Building
- `PowerShell -ExecutionPolicy RemoteSigned .\scripts\lint.ps1` - Run analyzers and build (Windows)
- `bash scripts/lint.sh` - Run analyzers and build (Unix/WSL)
- `dotnet build VRCQuestTools.sln` - Build entire solution
- `dotnet build VRCQuestTools*.csproj --no-incremental --verbosity quiet` - Build specific projects

### Testing
- Use Unity Test Runner window in Unity Editor
- Tests located in `Assets/VRCQuestTools-Tests/`
- Requires specific Unity environment setup

### Documentation (Website)
- `cd Website` - Navigate to documentation directory
- `pnpm install` - Install dependencies
- `pnpm start` - Start development server (English)
- `pnpm start -- --locale=ja` - Start development server (Japanese)
- `pnpm run build` - Build documentation
- `pnpm run serve` - Serve built documentation

### Package Management
- VPM (VRChat Package Manager) for VRChat packages
- pnpm for Node.js dependencies in Website/

## VSCode Tasks
Available tasks in VS Code (use Ctrl+Shift+P > "Tasks: Run Task"):
- `lint` - Run analyzers
- `start-website-en` - Start English documentation
- `start-website-ja` - Start Japanese documentation  
- `build-website` - Build documentation
- `test-website` - Build and serve documentation