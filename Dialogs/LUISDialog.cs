using HotelBot.Models.FacebookModels;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MyBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MyBot.Dialogs
{
    [LuisModel("xxxx", "xxx")]
    [Serializable]
    public class LUISDialog : LuisDialog<RoomReservation>
    {
        private readonly BuildFormDelegate<RoomReservation> ReserveRoom;

        [field: NonSerialized()]
        protected Activity _message;

        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _message = (Activity)await item;
            await base.MessageReceived(context, item);
        }

        public LUISDialog(BuildFormDelegate<RoomReservation> reserveRoom)
        {
            this.ReserveRoom = reserveRoom;
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry that I don't know what you mean.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            context.Call(new GreetingDialog(), Callback);
        }

        private async Task Callback(IDialogContext context, IAwaitable<object> result)
        {
            context.Wait(MessageReceived);
        }

        [LuisIntent("ReserveRoom")]
        private async Task RoomReservation(IDialogContext context, LuisResult result)
        {
            var enrollmentForm = new FormDialog<RoomReservation>(
                new RoomReservation(),
                this.ReserveRoom,
                FormOptions.PromptInStart
            );
            context.Call<RoomReservation>(enrollmentForm, Callback);
        }

        [LuisIntent("QueryAmenities")]
        public async Task QueryAmenities(IDialogContext context, LuisResult result)
        {
            foreach (var entity in result.Entities.Where(e => e.Type == "Amenity"))
            {
                var value = entity.Entity.ToLower();
                if (value == "pool" || value == "gym" || value == "wifi" || value == "towels")
                {
                    Activity replyMessage = _message.CreateReply();
                    var facebookMessage = new FacebookMessage();
                    facebookMessage.notification_type = "NO_PUSH";
                    facebookMessage.attachment = new FacebookAttachment();
                    facebookMessage.attachment.type = "template";
                    facebookMessage.attachment.payload = new FacebookPayload();
                    facebookMessage.attachment.payload.template_type = "generic";

                    var amenitiy = new FacebookElement();
                    amenitiy.subtitle = "Yes we have that!";
                    amenitiy.title = value;

                    switch (value)
                    {
                        case "pool":
                            amenitiy.image_url = "http://www.girltweetsworld.com/wp-content/uploads/2012/02/P1000180.jpg";
                            break;
                        case "gym":
                            amenitiy.image_url = "https://s-media-cache-ak0.pinimg.com/originals/cb/c9/4a/cbc94af79da9e334a8555e850da136f4.jpg";
                            break;
                        case "wifi":
                            amenitiy.image_url = "http://media.idownloadblog.com/wp-content/uploads/2016/02/wifi-icon.png";
                            break;
                        case "towels":
                            amenitiy.image_url = "http://www.prabhutextile.com/images/bath_towel_1.jpg";
                            break;
                        default:
                            break;
                    }
                    facebookMessage.attachment.payload.elements = new FacebookElement[] { amenitiy };
                    replyMessage.ChannelData = facebookMessage;

                    await context.PostAsync(replyMessage);

                    context.Wait(MessageReceived);
                    return;
                }
            }

            await context.PostAsync("I'm sorry that we don't have that.");
            context.Wait(MessageReceived);
        }
    }
}