#!/usr/bin/env python3
"""
Quick script to test connection to the Windows command server
"""
import os
import sys
import subprocess

def run_command(cmd):
    """Run a command and return its output"""
    print(f"Running: {cmd}")
    try:
        output = subprocess.check_output(cmd, shell=True, text=True)
        return output
    except subprocess.CalledProcessError as e:
        return f"Error: {e}"

def main():
    print("Testing Windows Command Server Connection")
    print("========================================")
    
    # Check if connector.py exists
    if not os.path.isfile("connector.py"):
        print("Error: connector.py not found in current directory")
        print("Make sure you're running this script from the directory containing connector.py")
        return 1
    
    # Make connector executable
    os.chmod("connector.py", 0o755)
    
    # Check connection
    print("\nChecking server connection...")
    output = run_command("./connector.py --check")
    print(output)
    
    if "Failed to connect" in output or "Error" in output:
        print("\nConnection failed. Please check your setup.")
        return 1
    
    # Try a simple command
    print("\nTrying a simple command (echo)...")
    output = run_command("./connector.py --cmd \"echo Connection successful!\"")
    print(output)
    
    print("\nAll tests completed.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
