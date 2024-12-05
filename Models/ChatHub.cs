using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace Health_Hub.Models
{


	public class ChatHub : Hub
	{
        // This method is called when a message is sent to a participant
        public async Task SendMessage(int recipientId, string message, string timeSent, string senderRole)
        {
            // Send the message to the specified recipient
            await Clients.All.SendAsync("ReceiveMessage", recipientId, message, timeSent, senderRole);
        }

        public async Task JoinChat(int chatId)
		{
			// Add the connection to the specific chat group
			await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
		}

		public async Task LeaveChat(int chatId)
		{
			// Remove the connection from the specific chat group
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
		}
	}

}
