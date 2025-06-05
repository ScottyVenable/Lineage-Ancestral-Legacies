# AI Connector Package

This repository contains everything needed for an AI running in an Ubuntu environment to connect to a Windows machine running the AI Code Assistant Command Server.

## Contents

- `connector.py` - The main Python script for connecting to the Windows command server
- `connect.sh` - Helper shell script with easy-to-use commands
- `test_connection.py` - Script to verify the connection is working
- `integration.py` - Example showing how to integrate the connector with AI code
- `SETUP_GUIDE.md` - Detailed setup instructions
- `README.md` - Overview and usage information
- `requirements.txt` - Required Python packages

## Quick Setup

1. Make the scripts executable:
   ```bash
   chmod +x connector.py connect.sh test_connection.py integration.py
   ```

2. Install requirements:
   ```bash
   pip install -r requirements.txt
   ```

3. Run the setup script and follow the prompts:
   ```bash
   ./connect.sh setup
   ```

4. Enter the Windows server IP address and API key when prompted.

## Usage Examples

### Check connection to server
```bash
./connect.sh check
```

### Run a command on the Windows machine
```bash
./connect.sh run "echo Hello from AI"
./connect.sh run "git status"
```

### Read a file from the Windows machine
```bash
./connect.sh read README.md
./connect.sh read src/main.py
```

### Write to a file on the Windows machine
```bash
./connect.sh write hello.txt "Hello from the AI assistant"
```

### List files on the Windows machine
```bash
./connect.sh list
./connect.sh list src
```

## For Advanced Users

You can also use the connector script directly:

```bash
# Check connection
./connector.py --check

# Execute command
./connector.py --cmd "git status"

# Read file
./connector.py --read README.md

# Write to file
./connector.py --write test.txt --content "Test content"

# List directory
./connector.py --list src
```

## Troubleshooting

If you have trouble connecting:

1. Ensure the Windows command server is running with external access enabled:
   ```
   python command_server.py --external
   ```

2. Check that the API key in your connector matches exactly with the key in the server.

3. Make sure there are no firewalls blocking the connection.

4. If you get path-related errors, you may need to modify the path conversion functions in the connector.py file.

## Security Note

This connector provides access to run commands and modify files on your Windows machine. Always use a strong API key and be cautious about the commands you allow to be executed.
