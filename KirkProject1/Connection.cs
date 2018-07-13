using System;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace KirkProject1
{
	public class Connection
	{
		protected TcpClient mClient;
		protected byte[] mReadBuffer;
		protected byte[] mWriteBuffer; 

		public byte[] ReadBuffer { get { return mReadBuffer; } }
		private int mBytesRead;
		public int BytesRead { get { return mBytesRead; } }

		public byte[] WriteBuffer { get { return mWriteBuffer; } }

		public TcpClient Client 
		{
			get { return mClient; }
			set { mClient = value; }
		}

		public Connection (TcpClient client)
		{
			Client = client;
			mWriteBuffer = new byte[16384];
			mReadBuffer = new byte[16384];
			mBytesRead = 0;
		}

		public bool TryRead(){
			// try to fill the read buffer
			// return true if there is still data available and everything went okay
			try{
				NetworkStream netStream = mClient.GetStream();
				if (netStream.DataAvailable){
					mBytesRead = netStream.Read(mReadBuffer, 0, mReadBuffer.Length);
				}
				return netStream.DataAvailable;
			} catch (Exception e){
				Console.WriteLine (e);
				return false;
			}
		}

		public void SendMessage(string message){
			NetworkStream stream = Client.GetStream();
			byte[] msg = System.Text.ASCIIEncoding.ASCII.GetBytes(message);
			Array.Copy (msg, mWriteBuffer, msg.Length);
			stream.BeginWrite (mWriteBuffer, 0, mWriteBuffer.Length, (res) => 
				{
					Array.Clear (mWriteBuffer, 0, mWriteBuffer.Length);
				}, stream);
			// stream.BeginWrite (mWriteBuffer, 0, mWriteBuffer.Length); // writing asynchronously was not helpful...
			//Array.Clear (mWriteBuffer, 0, mWriteBuffer.Length);
		}

		public void Disconnect(){
			SendMessage ("\nbye...\n");
			Client.Close ();
		}

		public byte[] PopReadBuffer(){
			// try to read something if there isn't anything read yet
			if (mBytesRead == 0) {
				TryRead ();
			}

			// then, if something was read, return the first section up to the first newline and keep the rest buffered
			if (mBytesRead > 0) {
				

				// find the first newline character
				int firstNewline = 0;
				while (mReadBuffer [firstNewline] != 10 && firstNewline < mReadBuffer.Length - 1) {
					firstNewline++;
				}
				byte[] toReturn = new byte[firstNewline-1];
				Array.Copy (mReadBuffer, 0, toReturn, 0, firstNewline-1);
				mBytesRead -= firstNewline + 1;
				if (mBytesRead < 0) {
					Array.Clear (mReadBuffer, 0, mReadBuffer.Length);
				} else {
					byte[] remainder = new byte[16384];
					Array.Copy (mReadBuffer, firstNewline + 1, remainder, 0, mBytesRead);
					Array.Clear (mReadBuffer, 0, mReadBuffer.Length);
					Array.Copy (remainder, 0, mReadBuffer, 0, mBytesRead);
				}
				return toReturn;
			}
			return null;
		}
	}
}

