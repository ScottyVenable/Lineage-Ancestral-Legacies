#!/bin/bash
# AI Helper Script
# This script makes it easier to use the command server from the Ubuntu AI environment

# Configuration
CONNECTOR_PATH="./connector.py"

# Check if the connector exists and is executable
if [ ! -f "$CONNECTOR_PATH" ]; then
    echo "Error: Connector script not found"
    echo "Please make sure connector.py exists in the same directory"
    exit 1
fi

# Make sure the connector is executable
chmod +x "$CONNECTOR_PATH" 2>/dev/null

# Function to show usage
function show_usage {
    echo "AI Helper for Windows Command Server"
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  run COMMAND      Execute a command on the Windows machine"
    echo "  read FILE        Read a file from the Windows machine"
    echo "  write FILE TEXT  Write TEXT to FILE on the Windows machine"
    echo "  list [DIR]       List contents of DIR (or workspace root if not specified)"
    echo "  check            Check connection to Windows command server"
    echo "  setup            Configure server connection settings"
    echo "  help             Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 run \"git status\""
    echo "  $0 read README.md"
    echo "  $0 write hello.txt \"Hello from AI\""
    echo "  $0 list"
}

# Function to set up the connector
function setup_connector {
    echo "Setting up AI connector..."
    echo ""
    
    # Get server IP and API key from user
    read -p "Enter Windows server IP address: " server_ip
    read -p "Enter API key for server authentication: " api_key
    
    # Validate input
    if [ -z "$server_ip" ] || [ -z "$api_key" ]; then
        echo "Error: Server IP and API key cannot be empty"
        exit 1
    fi
    
    # Create backup of connector file
    cp "$CONNECTOR_PATH" "${CONNECTOR_PATH}.bak" 2>/dev/null
    
    # Update connector file with sed
    sed -i "s/SERVER_HOST = \"YOUR_WINDOWS_IP_ADDRESS\"/SERVER_HOST = \"$server_ip\"/" "$CONNECTOR_PATH"
    sed -i "s/API_KEY = \"YOUR_SECRET_KEY\"/API_KEY = \"$api_key\"/" "$CONNECTOR_PATH"
    
    echo ""
    echo "Setup complete! Testing connection..."
    $CONNECTOR_PATH --check
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "Configuration successful. You can now use the connector."
    else
        echo ""
        echo "Connection failed. Please check your settings and try again."
        echo "You can run '$0 setup' again to reconfigure."
    fi
}

# Check connection to server
function check_connection {
    echo "Checking connection to Windows command server..."
    $CONNECTOR_PATH --check
    return $?
}

# No arguments, show help
if [ $# -eq 0 ]; then
    show_usage
    exit 0
fi

# Parse command
case "$1" in
    "help")
        show_usage
        ;;
    "setup")
        setup_connector
        ;;
    "check")
        check_connection
        ;;
    "run")
        if [ -z "$2" ]; then
            echo "Error: No command specified"
            echo "Usage: $0 run \"COMMAND\""
            exit 1
        fi
        $CONNECTOR_PATH --cmd "$2"
        ;;
    "read")
        if [ -z "$2" ]; then
            echo "Error: No file specified"
            echo "Usage: $0 read FILE"
            exit 1
        fi
        $CONNECTOR_PATH --read "$2"
        ;;
    "write")
        if [ -z "$2" ] || [ -z "$3" ]; then
            echo "Error: Missing file or content"
            echo "Usage: $0 write FILE CONTENT"
            exit 1
        fi
        $CONNECTOR_PATH --write "$2" --content "$3"
        ;;
    "list")
        if [ -z "$2" ]; then
            $CONNECTOR_PATH --list
        else
            $CONNECTOR_PATH --list "$2"
        fi
        ;;
    *)
        echo "Unknown command: $1"
        show_usage
        exit 1
        ;;
esac

exit $?
