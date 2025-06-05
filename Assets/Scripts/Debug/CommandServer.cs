using UnityEngine;
using System.Net;
using System.Threading;
using System;

namespace Lineage.Ancestral.Legacies.Debug
{
    /// <summary>
    /// HTTP server that allows remote AI code assistants to connect and send debug commands to Unity.
    /// Runs on a separate thread to avoid blocking the main Unity thread.
    /// </summary>
    public class CommandServer : MonoBehaviour
    {
        private HttpListener listener;
        private Thread listenerThread;

        void Start()
        {
            // The address to listen on. '+' means listen on all network interfaces
            // on port 8080. You can change the port.
            listener = new HttpListener();
            listener.Prefixes.Add("http://+:8080/"); 
            listener.Start();

            // Run the listener in a separate thread so it doesn't freeze the editor
            listenerThread = new Thread(Listen);
            listenerThread.Start();
            Debug.Log("Command Server Started on port 8080.");
        }

        private void Listen()
        {
            while (listener.IsListening)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    ProcessRequest(context);
                }
                catch (Exception e)
                {
                    // Catch exceptions when stopping the listener
                    Debug.Log(e.ToString());
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            string command = context.Request.Url.AbsolutePath;
            Debug.Log("Received command: " + command);

            // IMPORTANT: You need to dispatch the action back to the main Unity thread!
            // For example, you would use a queue to pass the command to the Update() method.
            // This is a simplified example. A real implementation needs thread-safe dispatching.
            
            // Example command handling
            if (command == "/runTest")
            {
                Debug.Log("Executing test command!");
                // UnityMainThreadDispatcher.Enqueue(() => { FindObjectOfType<Tests>().RunMyTest(); });
            }

            // Send a response back to the remote client
            string response = "Command received: " + command;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        void OnDestroy()
        {
            // Clean up when the game stops
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
                listenerThread.Join();
            }
        }
    }
}
