# Windows Command Server - Ubuntu AI Setup Guide

This guide explains how to set up the connection between your Ubuntu-based AI and a Windows machine running the AI Code Assistant Command Server.

## Prerequisites

1. A Windows machine running the AI Code Assistant Command Server
2. An Ubuntu 24.04 environment where your AI is running
3. Network connectivity between the two machines
4. Python 3.x installed on both machines

## Windows Server Setup

1. On your Windows machine, start the command server with external access enabled:

   ```powershell
   cd "path\to\command\server"
   python command_server.py --external
   ```

2. Note the IP address shown in the console output (e.g., 192.168.1.5)

3. Make note of the API key configured in the server (in `command_server.py`)

## Ubuntu AI Setup

1. Copy these files to your Ubuntu environment:
   - `connector.py` - The main connector script
   - `connect.sh` - A helper shell script
   - `README.md` - Documentation
   - `test_connection.py` - Connection testing script

2. Make the scripts executable:

   ```bash
   chmod +x connector.py connect.sh test_connection.py
   ```

3. Configure the connector with your Windows server information:

   ```bash
   ./connect.sh setup
   ```

   Enter the Windows IP address and API key when prompted.

4. Test the connection:

   ```bash
   ./test_connection.py
   ```

## Integrating with Your AI

### Option 1: Direct Script Execution

Your AI can directly execute the connect.sh script:

```bash
./connect.sh run "git status"
./connect.sh read README.md
```

### Option 2: Python Integration

Your AI can import the connector module and use its functions:

```python
import subprocess

def run_windows_command(command):
    result = subprocess.check_output(['./connector.py', '--cmd', command], text=True)
    return result

def read_windows_file(filepath):
    result = subprocess.check_output(['./connector.py', '--read', filepath], text=True)
    return result

# Example usage
print(run_windows_command("git status"))
print(read_windows_file("README.md"))
```

### Option 3: System Calls

Your AI can make system calls using the helper script:

```
system('./connect.sh run "git status"')
system('./connect.sh read README.md')
```

## Troubleshooting

1. **Connection Issues**
   - Verify the Windows server is running with `--external` flag
   - Check that the IP address is correct
   - Ensure there are no firewall blocks

2. **Authentication Issues**
   - Double-check the API key matches exactly with the server
   - Look for any spaces or special characters that might be causing issues

3. **Path Conversion Problems**
   - If you encounter path-related errors, you may need to adjust the path conversion functions in `connector.py`

## Security Recommendations

1. Use a strong, unique API key
2. Consider using a VPN or SSH tunnel for additional security
3. Restrict the allowed commands on the Windows server to only what's necessary
4. Don't leave the Windows server running when not in use
