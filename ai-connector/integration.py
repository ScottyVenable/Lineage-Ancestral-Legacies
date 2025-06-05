#!/usr/bin/env python3
"""
Integration example for AI assistants
This demonstrates how to integrate the connector with AI code
"""
import os
import subprocess
import sys

# Define base folder for connector scripts
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

class WindowsConnector:
    """Class to handle Windows command server interactions"""
    
    def __init__(self):
        """Initialize the connector"""
        self.connector_path = os.path.join(SCRIPT_DIR, "connector.py")
        
        # Ensure connector is executable
        if os.path.exists(self.connector_path):
            os.chmod(self.connector_path, 0o755)
    
    def execute_command(self, command):
        """Execute a command on the Windows machine"""
        try:
            result = subprocess.check_output(
                [self.connector_path, "--cmd", command],
                text=True,
                stderr=subprocess.STDOUT
            )
            return result
        except subprocess.CalledProcessError as e:
            return f"Error ({e.returncode}): {e.output}"
    
    def read_file(self, file_path):
        """Read a file from the Windows machine"""
        try:
            result = subprocess.check_output(
                [self.connector_path, "--read", file_path],
                text=True,
                stderr=subprocess.STDOUT
            )
            return result
        except subprocess.CalledProcessError as e:
            return f"Error ({e.returncode}): {e.output}"
    
    def write_file(self, file_path, content):
        """Write content to a file on the Windows machine"""
        try:
            result = subprocess.check_output(
                [self.connector_path, "--write", file_path, "--content", content],
                text=True,
                stderr=subprocess.STDOUT
            )
            return result
        except subprocess.CalledProcessError as e:
            return f"Error ({e.returncode}): {e.output}"
    
    def list_directory(self, dir_path=None):
        """List contents of a directory on the Windows machine"""
        try:
            if dir_path:
                result = subprocess.check_output(
                    [self.connector_path, "--list", dir_path],
                    text=True,
                    stderr=subprocess.STDOUT
                )
            else:
                result = subprocess.check_output(
                    [self.connector_path, "--list"],
                    text=True,
                    stderr=subprocess.STDOUT
                )
            return result
        except subprocess.CalledProcessError as e:
            return f"Error ({e.returncode}): {e.output}"

# Example usage
def example_usage():
    """Example of how to use the WindowsConnector class"""
    windows = WindowsConnector()
    
    # Execute a command
    print("Executing command: 'echo Hello from AI'")
    result = windows.execute_command("echo Hello from AI")
    print(f"Result: {result}\n")
    
    # List directory
    print("Listing directory contents:")
    result = windows.list_directory()
    print(f"Result: {result}\n")
    
    # Write a file
    print("Writing to test.txt")
    result = windows.write_file("test.txt", "This is a test file created by the AI")
    print(f"Result: {result}\n")
    
    # Read the file back
    print("Reading test.txt")
    result = windows.read_file("test.txt")
    print(f"Result: {result}\n")

if __name__ == "__main__":
    print("Windows Connector Integration Example")
    print("===================================\n")
    example_usage()
