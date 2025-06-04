Based on the #codebase on how we structure things and organize, what instructions would you give yourself  to use for each type of generation request (review, code generation, commit message generation, code generation). Here are summaries of what each instruction setting in settings.json does:

---
GitHub › Copilot › Chat › Commit Message Generation: Instructions
A set of instructions that will be added to Copilot requests that generate commit messages. Instructions can come from:

a file in the workspace: { "file": "fileName" }
text in natural language: { "text": "Use conventional commit message format." }
Note: Keep your instructions short and precise. Poor instructions can degrade Copilot's quality and performance.

---

GitHub › Copilot › Chat › Review Selection: Instructions
A set of instructions that will be added to Copilot requests that provide code review for the current selection. Instructions can come from:

a file in the workspace: { "file": "fileName" }
text in natural language: { "text": "Use underscore for field names." }
Note: Keep your instructions short and precise. Poor instructions can degrade Copilot's effectiveness.

---

GitHub › Copilot › Chat › Code Generation: Instructions
A set of instructions that will be added to Copilot requests that generate code. Instructions can come from:

a file in the workspace: { "file": "fileName" }
text in natural language: { "text": "Use underscore for field names." }
Note: Keep your instructions short and precise. Poor instructions can degrade Copilot's quality and performance.

---

GitHub › Copilot › Chat › Test Generation: Instructions
A set of instructions that will be added to Copilot requests that generate tests. Instructions can come from:

a file in the workspace: { "file": "fileName" }
text in natural language: { "text": "Use underscore for field names." }
Note: Keep your instructions short and precise. Poor instructions can degrade Copilot's quality and performance.

---

Also some quick notes to add to the instructions:

1. When running commands, they should be formatted to be for PowerShell.
2. When running commands, the most efficient way to run them is required. For example, moving multiple files should try to move them all at once instead of moving them one at a time with one command for each move.
