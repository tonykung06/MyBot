using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace MyBot.Dialogs
{
    [Serializable]
    public class GreetingDialog : IDialog
    {
        public async Task StartAsync(IDialogContext context)
        {
            await Respond(context);
        }

        private async Task Respond(IDialogContext context)
        {
            var userName = String.Empty;
            context.UserData.TryGetValue<string>("Name", out userName);
            if (string.IsNullOrEmpty(userName))
            {
                await context.PostAsync("Hi this is Tony bot. What is your name?");
                context.Wait(AskForName);
            }
            else
            {
                await context.PostAsync(String.Format("Hey {0}, this is Tony bot. What can I help?", userName));
                context.Done("done");
            }
        }

        public virtual async Task AskForName(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var userName = message.Text;
            context.UserData.SetValue<string>("Name", userName);
            await Respond(context);
        }
    }
}