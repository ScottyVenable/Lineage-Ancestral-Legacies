#!/usr/bin/env python3
"""
AI Command Server Connector
This script allows an AI running in Ubuntu to connect to a Windows-based command server.
"""
import requests
import os
import json
import sys
import argparse

# Configuration - MODIFY THESE VALUES
SERVER_HOST = "192.169.7.67"  # Replace with the Windows PC's IP address
SERVER_PORT = 5000
SERVER_URL = f"http://{SERVER_HOST}:{SERVER_PORT}"
API_KEY = "lineage"  # Replace with the API key configured in the server

# Headers for authentication
HEADERS = {
    "Content-Type": "application/json",
    "X-API-Key": API_KEY
}

# Path conversion between Windows and Linux
def convert_to_windows_path(linux_path):
    """Convert a Linux path to a Windows path format if needed"""
    # Remove leading slash and convert to Windows format with C: drive
    if linux_path.startswith("/"):
        return "C:" + linux_path.replace("/", "\\")
    return linux_path

def convert_from_windows_path(windows_path):
    """Convert a Windows path to Linux format for display"""
    # This is simplified - in a real implementation, you'd need mapping rules
    if ":" in windows_path:
        # Remove drive letter and convert backslashes to forward slashes
        return "/" + windows_path.split(":", 1)[1].replace("\\", "/")
    return windows_path.replace("\\", "/")

def execute_command(command, cwd=None):
    """Execute a command on the Windows command server"""
    payload = {
        "command": command
    }
    if cwd:
        # Convert to Windows-style path if it's a Linux-style path
        payload["cwd"] = convert_to_windows_path(cwd)
        
    print(f"Sending command to server: {command}")
    
    try:
        response = requests.post(
            f"{SERVER_URL}/command", 
            headers=HEADERS, 
            json=payload,
            timeout=30
        )
        
        if response.status_code == 200:
            result = response.json()
            output = ""
            if result.get('stdout'):
                output += f"{result['stdout']}"
            if result.get('stderr'):
                output += f"\nERROR OUTPUT:\n{result['stderr']}"
            return output
        else:
            return f"Error {response.status_code}: {response.text}"
    except requests.RequestException as e:
        return f"Connection error: {str(e)}"

def read_file(file_path):
    """Read a file from the Windows command server"""
    # Convert to Windows path format if it's a Linux-style path
    windows_path = convert_to_windows_path(file_path)
    
    try:
        response = requests.get(
            f"{SERVER_URL}/file/read?path={windows_path}", 
            headers=HEADERS,
            timeout=10
        )
        
        if response.status_code == 200:
            result = response.json()
            return result["content"]
        else:
            return f"Error {response.status_code}: {response.text}"
    except requests.RequestException as e:
        return f"Connection error: {str(e)}"

def write_file(file_path, content):
    """Write to a file on the Windows command server"""
    # Convert to Windows path format if it's a Linux-style path
    windows_path = convert_to_windows_path(file_path)
    
    payload = {
        "path": windows_path,
        "content": content
    }
    
    try:
        response = requests.post(
            f"{SERVER_URL}/file/write", 
            headers=HEADERS, 
            json=payload,
            timeout=10
        )
        
        if response.status_code == 200:
            result = response.json()
            return f"File written: {result['path']} ({result['size']} bytes)"
        else:
            return f"Error {response.status_code}: {response.text}"
    except requests.RequestException as e:
        return f"Connection error: {str(e)}"

def list_directory(dir_path=None):
    """List contents of a directory on the Windows command server"""
    url = f"{SERVER_URL}/workspace/list"
    
    if dir_path:
        # Convert to Windows path format if it's a Linux-style path
        windows_path = convert_to_windows_path(dir_path)
        url += f"?path={windows_path}"
    
    try:
        response = requests.get(url, headers=HEADERS, timeout=10)
        
        if response.status_code == 200:
            result = response.json()
            
            # Convert Windows paths to Linux format for display
            linux_path = convert_from_windows_path(result["path"])
            output = f"Directory: {linux_path}\n"
            output += "Contents:\n"
            
            # Sort items: directories first, then files
            items = sorted(result["contents"], key=lambda x: (0 if x["is_directory"] else 1, x["name"].lower()))
            
            for item in items:
                item_type = "DIR" if item["is_directory"] else "FILE"
                size = f"{item['size']} bytes" if item["size"] is not None else ""
                output += f"  [{item_type}] {item['name']} {size}\n"
            return output
        else:
            return f"Error {response.status_code}: {response.text}"
    except requests.RequestException as e:
        return f"Connection error: {str(e)}"

def check_server_connection():
    """Check if the Windows command server is running and accessible"""
    try:
        response = requests.get(f"{SERVER_URL}/vscode/status", headers=HEADERS, timeout=5)
        if response.status_code == 200:
            result = response.json()
            print(f"Connected to server at {SERVER_URL}")
            print(f"Workspace root: {convert_from_windows_path(result['workspace_root'])}")
            print(f"OS type: {result['os_type']}")
            print(f"API version: {result.get('api_version', 'unknown')}")
            return True
        else:
            print(f"Error connecting to server: {response.status_code} - {response.text}")
            return False
    except Exception as e:
        print(f"Failed to connect to server at {SERVER_URL}: {str(e)}")
        return False

def main():
    """Main function that handles command-line arguments"""
    parser = argparse.ArgumentParser(description="AI Command Server Connector")
    
    parser.add_argument('--check', action='store_true', help='Check connection to the Windows command server')
    parser.add_argument('--cmd', help='Execute a command on the Windows server')
    parser.add_argument('--read', help='Read a file from the Windows server')
    parser.add_argument('--write', help='Path to write a file on the Windows server')
    parser.add_argument('--content', help='Content to write to the file')
    parser.add_argument('--list', nargs='?', const='.', help='List directory contents (default: workspace root)')
    
    args = parser.parse_args()
    
    # First, check configuration
    if SERVER_HOST == "YOUR_WINDOWS_IP_ADDRESS" or API_KEY == "YOUR_SECRET_KEY":
        print("ERROR: You need to configure the script first!")
        print("1. Open this script in a text editor")
        print("2. Change SERVER_HOST to your Windows PC's IP address")
        print("3. Change API_KEY to match your command server's key")
        print("4. Save and try again")
        return 1
    
    # If no arguments provided or just --check
    if len(sys.argv) == 1 or args.check:
        if check_server_connection():
            print("\nServer connection successful!")
            return 0
        else:
            print("\nFailed to connect to the server.")
            return 1
    
    # Execute command
    if args.cmd:
        print(f"Executing command: {args.cmd}")
        output = execute_command(args.cmd)
        print("\nCommand output:")
        print("--------------")
        print(output)
    
    # Read file
    elif args.read:
        print(f"Reading file: {args.read}")
        content = read_file(args.read)
        print("\nFile content:")
        print("------------")
        print(content)
    
    # Write file
    elif args.write and args.content:
        print(f"Writing to file: {args.write}")
        result = write_file(args.write, args.content)
        print(result)
    
    # List directory
    elif args.list:
        path = None if args.list == '.' else args.list
        print(f"Listing directory: {path or 'workspace root'}")
        output = list_directory(path)
        print("\nDirectory listing:")
        print("-----------------")
        print(output)
    
    else:
        parser.print_help()
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
