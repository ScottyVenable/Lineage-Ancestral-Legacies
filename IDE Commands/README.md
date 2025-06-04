# Development Command Tools

This folder contains PowerShell scripts to optimize Unity development workflow in VS Code.

## 🚀 Main Entry Point

**`dev-hub.ps1`** - Central command hub with interactive menu
```powershell
.\dev-hub.ps1                    # Interactive mode with menu
.\dev-hub.ps1 quick build        # Direct command execution
.\dev-hub.ps1 status             # Quick project status
```

## 🛠️ Individual Tools

### `dev-quick.ps1` - Essential Development Commands
Fast access to common development tasks:
- **Build/Clean**: `.\dev-quick.ps1 build` | `clean` | `reset`
- **Code Access**: `.\dev-quick.ps1 entity` | `entitydata` | `docs`
- **Status**: `.\dev-quick.ps1 status` | `git` | `mem` | `errors`
- **Search**: `.\dev-quick.ps1 find [term]`
- **Performance**: `.\dev-quick.ps1 monitor` | `cache-clear`

### `code-nav.ps1` - Code Navigation & Search
Advanced code exploration:
- **Search**: `.\code-nav.ps1 class [name]` | `method [name]` | `usage [term]`
- **Comments**: `.\code-nav.ps1 todo` | `fixme`
- **Files**: `.\code-nav.ps1 recent` | `large` | `duplicates` | `scripts`
- **Entity System**: `.\code-nav.ps1 entity` | `entity-deps` | `stats`

### `git-flow.ps1` - Git Workflow Optimization
Streamlined git operations:
- **Status**: `.\git-flow.ps1 status` | `log` | `diff` | `branches`
- **Commits**: `.\git-flow.ps1 quick [msg]` | `backup` | `checkpoint`
- **Branches**: `.\git-flow.ps1 feature [name]` | `switch [branch]`
- **Safety**: `.\git-flow.ps1 stash` | `unstash` | `reset-soft`

### `test-tools.ps1` - Testing & Validation
Code quality and validation:
- **Compilation**: `.\test-tools.ps1 compile` | `syntax` | `missing-refs`
- **Entity System**: `.\test-tools.ps1 entity-validate` | `entity-stats`
- **Analysis**: `.\test-tools.ps1 complexity` | `metrics` | `unused`
- **Unity Specific**: `.\test-tools.ps1 meta-check` | `behavior-check`

## 📊 Performance Monitoring (Existing)
- `unity-performance-monitor.ps1` - Real-time Unity performance monitoring
- `ui-work-monitor.ps1` - UI-specific performance tracking
- `unity-health-check.ps1` - System health validation

## 🚀 Quick Start Examples

```powershell
# Start interactive hub
.\dev-hub.ps1

# Quick build and status check
.\dev-hub.ps1 quick build
.\dev-hub.ps1 status

# Find and edit Entity code
.\dev-hub.ps1 nav entity
.\dev-hub.ps1 quick entity

# Git workflow
.\dev-hub.ps1 git status
.\dev-hub.ps1 git quick "Fixed entity stats"

# Code validation
.\dev-hub.ps1 test compile
.\dev-hub.ps1 test entity-validate

# Performance monitoring
.\unity-performance-monitor.ps1
```

## 💡 Pro Tips

1. **Use the Hub**: Start with `.\dev-hub.ps1` for the interactive menu
2. **Quick Actions**: Use Q1-Q6 in the hub for instant common tasks
3. **Command Chaining**: Chain commands with `&&` for workflows
4. **Tab Completion**: PowerShell supports tab completion for parameters
5. **Background Monitoring**: Use performance monitors while coding

## 🔧 Customization

Each script supports a `help` parameter to see all available options:
```powershell
.\dev-quick.ps1 help
.\code-nav.ps1 help
.\git-flow.ps1 help
.\test-tools.ps1 help
```

## 📁 File Organization

- All scripts are in the `IDE Commands/` folder
- Scripts are designed to run from the Unity project root
- Each script automatically navigates to the correct directory
- All commands preserve your current working directory

---

**Created**: June 2025  
**Purpose**: Optimize Unity development workflow in VS Code  
**Compatibility**: PowerShell 5.0+ on Windows
