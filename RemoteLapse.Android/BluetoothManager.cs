using System.IO;
using System.Runtime.CompilerServices;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace RemoteLapse.Droid
{
    enum BluetoothType
    {
       
		// TODO: Make into Enums
		MESSAGE_STATE_CHANGE = 1,
		MESSAGE_READ = 2,
		MESSAGE_WRITE = 3,
		MESSAGE_DEVICE_NAME = 4,
		MESSAGE_TOAST = 5

    }
 
    enum BluetoothConnectionState
    {
        // Constants that indicate the current connection state
		STATE_NONE = 0,       // we're doing nothing
		STATE_LISTEN = 1,     // now listening for incoming connections
		STATE_CONNECTING = 2, // now initiating an outgoing connection
		STATE_CONNECTED = 3,  // now connected to a remote device
    }
    class BluetoothManager
    {
       // Debugging
		private const string TAG = "BluetoothChatService";
		private const bool Debug = true;
	
		// Name for the SDP record when creating server socket
		private const string NAME = "BluetoothChat";
	
		// Unique UUID for this application
		private static UUID MY_UUID = UUID.FromString ("fa87c0d0-afac-11de-8a39-0800200c9a66");
		
		// Member fields
		protected BluetoothAdapter _adapter;
		protected Handler _handler;
		private AcceptThread acceptThread;
		protected ConnectThread connectThread;
		private ConnectedThread connectedThread;
        protected BluetoothConnectionState _state;
	
		
		/// <summary>
		/// Constructor. Prepares a new BluetoothChat session.
		/// </summary>
		/// <param name='context'>
		/// The UI Activity Context.
		/// </param>
		/// <param name='handler'>
		/// A Handler to send messages back to the UI Activity.
		/// </param>
        public BluetoothManager(Context context, Handler handler)
		{
			_adapter = BluetoothAdapter.DefaultAdapter;
            _state = BluetoothConnectionState.STATE_NONE;
			_handler = handler;
		}
		
		/// <summary>
		/// Set the current state of the chat connection.
		/// </summary>
		/// <param name='state'>
		/// An integer defining the current connection state.
		/// </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
        private void SetState(BluetoothConnectionState state)
		{
			if (Debug)
				Log.Debug (TAG, "setState() " + _state + " -> " + state);
			
			_state = state;
	
			// Give the new state to the Handler so the UI Activity can update
			_handler.ObtainMessage ((int)BluetoothType.MESSAGE_STATE_CHANGE, (int)state, -1).SendToTarget ();
		}
		
		/// <summary>
		/// Return the current connection state.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
        public BluetoothConnectionState GetState()
		{
			return _state;
		}
		
		// Start the chat service. Specifically start AcceptThread to begin a
		// session in listening (server) mode. Called by the Activity onResume()
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Start ()
		{	
			if (Debug)
				Log.Debug (TAG, "start");
	
			// Cancel any thread attempting to make a connection
			if (connectThread != null) {
				connectThread.Cancel ();
				connectThread = null;
			}
	
			// Cancel any thread currently running a connection
			if (connectedThread != null) {
				connectedThread.Cancel ();
				connectedThread = null;
			}
	
			// Start the thread to listen on a BluetoothServerSocket
			if (acceptThread == null) {
				acceptThread = new AcceptThread (this);
				acceptThread.Start ();
			}

            SetState(BluetoothConnectionState.STATE_LISTEN);
		}
		
		/// <summary>
		/// Start the ConnectThread to initiate a connection to a remote device.
		/// </summary>
		/// <param name='device'>
		/// The BluetoothDevice to connect.
		/// </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Connect (BluetoothDevice device)
		{
			if (Debug)
				Log.Debug (TAG, "connect to: " + device);
	
			// Cancel any thread attempting to make a connection
            if (_state == BluetoothConnectionState.STATE_CONNECTING)
            {
				if (connectThread != null) {
					connectThread.Cancel ();
					connectThread = null;
				}
			}
	
			// Cancel any thread currently running a connection
			if (connectedThread != null) {
				connectedThread.Cancel ();
				connectedThread = null;
			}
	
			// Start the thread to connect with the given device
			connectThread = new ConnectThread(device, this);
			connectThread.Start ();

            SetState(BluetoothConnectionState.STATE_CONNECTING);
		}
		
		/// <summary>
		/// Start the ConnectedThread to begin managing a Bluetooth connection
		/// </summary>
		/// <param name='socket'>
		/// The BluetoothSocket on which the connection was made.
		/// </param>
		/// <param name='device'>
		/// The BluetoothDevice that has been connected.
		/// </param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Connected (BluetoothSocket socket, BluetoothDevice device)
		{
			if (Debug)
				Log.Debug (TAG, "connected");
	
			// Cancel the thread that completed the connection
			if (connectThread != null) {
				connectThread.Cancel ();
				connectThread = null;
			}
	
			// Cancel any thread currently running a connection
			if (connectedThread != null) {
				connectedThread.Cancel ();
				connectedThread = null;
			}
	
			// Cancel the accept thread because we only want to connect to one device
			if (acceptThread != null) {
				acceptThread.Cancel ();
				acceptThread = null;
			}
			
			// Start the thread to manage the connection and perform transmissions
			connectedThread = new ConnectedThread (socket, this);
			connectedThread.Start ();
	
			// Send the name of the connected device back to the UI Activity
            var msg = _handler.ObtainMessage((int)BluetoothType.MESSAGE_DEVICE_NAME);
			Bundle bundle = new Bundle ();
			bundle.PutString (MainActivity.DEVICE_NAME, device.Name);
			msg.Data = bundle;
			_handler.SendMessage (msg);

            SetState(BluetoothConnectionState.STATE_CONNECTED);
		}
		
		/// <summary>
		/// Stop all threads.
		/// </summary>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Stop ()
		{
			if (Debug)
				Log.Debug (TAG, "stop");
	
			if (connectThread != null) {
				connectThread.Cancel ();
				connectThread = null;
			}
	
			if (connectedThread != null) {
				connectedThread.Cancel ();
				connectedThread = null;
			}
	
			if (acceptThread != null) {
				acceptThread.Cancel ();
				acceptThread = null;
			}

            SetState(BluetoothConnectionState.STATE_NONE);
		}
		
		/// <summary>
		/// Write to the ConnectedThread in an unsynchronized manner
		/// </summary>
		/// <param name='out'>
		/// The bytes to write.
		/// </param>
		public void Write (byte[] @out)
		{
			// Create temporary object
			ConnectedThread r;
			// Synchronize a copy of the ConnectedThread
			lock (this) {
                if (_state != BluetoothConnectionState.STATE_CONNECTED)
					return;
				r = connectedThread;
			}
			// Perform the write unsynchronized
			r.Write (@out);
		}
	
		/// <summary>
		/// Indicate that the connection attempt failed and notify the UI Activity.
		/// </summary>
		private void ConnectionFailed ()
		{
            SetState(BluetoothConnectionState.STATE_LISTEN);
			
			// Send a failure message back to the Activity
			var msg = _handler.ObtainMessage ((int)BluetoothType.MESSAGE_TOAST);
			Bundle bundle = new Bundle ();
			bundle.PutString (MainActivity.TOAST, "Unable to connect device");
			msg.Data = bundle;
			_handler.SendMessage (msg);
		}
	
		/// <summary>
		/// Indicate that the connection was lost and notify the UI Activity.
		/// </summary>
		public void ConnectionLost ()
		{
            SetState(BluetoothConnectionState.STATE_LISTEN);
			
			// Send a failure message back to the Activity
            var msg = _handler.ObtainMessage((int)BluetoothType.MESSAGE_TOAST);
			Bundle bundle = new Bundle ();
            bundle.PutString(MainActivity.TOAST, "Device connection was lost");
			msg.Data = bundle;
			_handler.SendMessage (msg);
		}

		/// <summary>
		/// This thread runs while listening for incoming connections. It behaves
		/// like a server-side client. It runs until a connection is accepted
		/// (or until cancelled).
		/// </summary>
		// TODO: Convert to a .NET thread
		private class AcceptThread : Thread
		{
			// The local server socket
			private BluetoothServerSocket mmServerSocket;
			private BluetoothManager _service;

            public AcceptThread(BluetoothManager service)
			{
				_service = service;
				BluetoothServerSocket tmp = null;
	
				// Create a new listening server socket
				try {
					tmp = _service._adapter.ListenUsingRfcommWithServiceRecord (NAME, MY_UUID);
	
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "listen() failed", e);
				}
				mmServerSocket = tmp;
			}
	
			public override void Run ()
			{
				if (Debug)
					Log.Debug (TAG, "BEGIN mAcceptThread " + this.ToString ());
				
				Name = "AcceptThread";
				BluetoothSocket socket = null;
	
				// Listen to the server socket if we're not connected
				while (_service._state != BluetoothConnectionState.STATE_CONNECTED) {
					try {
						// This is a blocking call and will only return on a
						// successful connection or an exception
						socket = mmServerSocket.Accept ();
					} catch (Java.IO.IOException e) {
						Log.Error (TAG, "accept() failed", e);
						break;
					}
					
					// If a connection was accepted
					if (socket != null) {
						lock (this) {
							switch (_service._state) {
							case BluetoothConnectionState.STATE_LISTEN:
                            case BluetoothConnectionState.STATE_CONNECTING:
								// Situation normal. Start the connected thread.
								_service.Connected (socket, socket.RemoteDevice);
								break;
                            case BluetoothConnectionState.STATE_NONE:
                            case BluetoothConnectionState.STATE_CONNECTED:
								// Either not ready or already connected. Terminate new socket.
								try {
									socket.Close ();
								} catch (Java.IO.IOException e) {
									Log.Error (TAG, "Could not close unwanted socket", e);
								}
								break;
							}
						}
					}
				}
				
				if (Debug)
					Log.Info (TAG, "END mAcceptThread");
			}
	
			public void Cancel ()
			{
				if (Debug)
					Log.Debug (TAG, "cancel " + this.ToString ());
				
				try {
					mmServerSocket.Close ();
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "close() of server failed", e);
				}
			}
		}
		
		/// <summary>
		/// This thread runs while attempting to make an outgoing connection
		/// with a device. It runs straight through; the connection either
		/// succeeds or fails.
		/// </summary>
		// TODO: Convert to a .NET thread
		protected class ConnectThread : Thread
		{
			private BluetoothSocket mmSocket;
			private BluetoothDevice mmDevice;
            private BluetoothManager _service;
			
			public ConnectThread (BluetoothDevice device, BluetoothManager service)
			{
				mmDevice = device;
				_service = service;
				BluetoothSocket tmp = null;
				
				// Get a BluetoothSocket for a connection with the
				// given BluetoothDevice
				try {
					tmp = device.CreateRfcommSocketToServiceRecord (MY_UUID);
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "create() failed", e);
				}
				mmSocket = tmp;
			}
			
			public override void Run ()
			{
				Log.Info (TAG, "BEGIN mConnectThread");
				Name = "ConnectThread";
	
				// Always cancel discovery because it will slow down a connection
				_service._adapter.CancelDiscovery ();
	
				// Make a connection to the BluetoothSocket
				try {
					// This is a blocking call and will only return on a
					// successful connection or an exception
					mmSocket.Connect ();
				} catch (Java.IO.IOException e) {
					_service.ConnectionFailed ();
					// Close the socket
					try {
						mmSocket.Close ();
					} catch (Java.IO.IOException e2) {
						Log.Error (TAG, "unable to close() socket during connection failure", e2);
					}

					// Start the service over to restart listening mode
					_service.Start ();
					return;
				}
	
				// Reset the ConnectThread because we're done
				lock (this) {
					_service.connectThread = null;
				}
	
				// Start the connected thread
				_service.Connected (mmSocket, mmDevice);
			}
	
			public void Cancel ()
			{
				try {
					mmSocket.Close ();
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "close() of connect socket failed", e);
				}
			}
		}
	
		/// <summary>
		/// This thread runs during a connection with a remote device.
		/// It handles all incoming and outgoing transmissions.
		/// </summary>
		// TODO: Convert to a .NET thread
		private class ConnectedThread : Thread
		{
			private BluetoothSocket mmSocket;
			private Stream mmInStream;
			private Stream mmOutStream;
			private BluetoothManager _service;

            public ConnectedThread(BluetoothSocket socket, BluetoothManager service)
			{
				Log.Debug (TAG, "create ConnectedThread: ");
				mmSocket = socket;
				_service = service;
				Stream tmpIn = null;
				Stream tmpOut = null;
	
				// Get the BluetoothSocket input and output streams
				try {
					tmpIn = socket.InputStream;
					tmpOut = socket.OutputStream;
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "temp sockets not created", e);
				}
	
				mmInStream = tmpIn;
				mmOutStream = tmpOut;
			}
	
			public override void Run ()
			{
				Log.Info (TAG, "BEGIN mConnectedThread");
				byte[] buffer = new byte[1024];
				int bytes;
	
				// Keep listening to the InputStream while connected
				while (true) {
					try {
						// Read from the InputStream
						bytes = mmInStream.Read (buffer, 0, buffer.Length);
	
						// Send the obtained bytes to the UI Activity
						_service._handler.ObtainMessage ((int)BluetoothType.MESSAGE_READ, bytes, -1, buffer)
							.SendToTarget ();
					} catch (Java.IO.IOException e) {
						Log.Error (TAG, "disconnected", e);
						_service.ConnectionLost ();
						break;
					}
				}
			}
	
			/// <summary>
			/// Write to the connected OutStream.
			/// </summary>
			/// <param name='buffer'>
			/// The bytes to write
			/// </param>
			public void Write (byte[] buffer)
			{
				try {
					mmOutStream.Write (buffer, 0, buffer.Length);
	
					// Share the sent message back to the UI Activity
                    _service._handler.ObtainMessage((int)BluetoothType.MESSAGE_WRITE, -1, -1, buffer)
						.SendToTarget ();
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "Exception during write", e);
				}
			}
	
			public void Cancel ()
			{
				try {
					mmSocket.Close ();
				} catch (Java.IO.IOException e) {
					Log.Error (TAG, "close() of connect socket failed", e);
				}
			}
		}

    }
}