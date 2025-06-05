import subprocess
from flask import Flask, request, jsonify

# Create the Flask web server application
app = Flask(__name__)

# --- SECURITY ---
# A whitelist of commands that are safe to be executed by this server.
# The command from the request MUST start with one of these strings.
ALLOWED_COMMANDS = [
    "git status",
    "git pull",
    "ls -l",
    "echo Hello" 
]

def is_command_allowed(command):
    """Checks if the received command is in our whitelist."""
    # We use startswith to allow for arguments, e.g., "ls -l /path/to/dir"
    # if "ls -l" is in the allowed list.
    for allowed in ALLOWED_COMMANDS:
        if command.strip().startswith(allowed):
            return True
    return False

# Define the '/command' endpoint that will receive our commands
@app.route('/command', methods=['POST'])
def execute_command():
    # Get the JSON data from the incoming request
    data = request.get_json()

    # Check if the JSON data exists and has a 'command' key
    if not data or 'command' not in data:
        return jsonify({"error": "Invalid request. Missing 'command' key."}), 400

    command_to_run = data['command']

    # --- SECURITY CHECK ---
    if not is_command_allowed(command_to_run):
        print(f"Denied forbidden command: {command_to_run}")
        return jsonify({"error": f"Command not allowed: '{command_to_run}'"}), 403 # 403 Forbidden

    print(f"Executing allowed command: {command_to_run}")

    try:
        # Execute the command in the shell
        # capture_output=True saves the command's stdout and stderr
        # text=True decodes them as text
        # shell=True is needed to process commands like 'ls -l', but be cautious
        result = subprocess.run(
            command_to_run, 
            shell=True, 
            capture_output=True, 
            text=True, 
            check=True # This will raise an exception if the command returns a non-zero exit code
        )

        # Return the output in a JSON response
        return jsonify({
            "command": command_to_run,
            "stdout": result.stdout,
            "stderr": result.stderr
        })

    except subprocess.CalledProcessError as e:
        # This handles cases where the command itself fails
        return jsonify({
            "error": "Command failed to execute.",
            "command": command_to_run,
            "stdout": e.stdout,
            "stderr": e.stderr
        }), 500
    except Exception as e:
        # Catch any other unexpected errors
        return jsonify({"error": str(e)}), 500

# This line allows you to run the script directly
if __name__ == '__main__':
    # Runs the server on localhost (127.0.0.1) at port 5000
    app.run(host='127.0.0.1', port=5000, debug=True)