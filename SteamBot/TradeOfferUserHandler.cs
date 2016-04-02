using SteamKit2;
using SteamTrade;
using SteamTrade.TradeOffer;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TradeAsset = SteamTrade.TradeOffer.TradeOffer.TradeStatusUser.TradeAsset;

namespace SteamBot
{
    public class TradeOfferUserHandler : UserHandler
    {
        public TradeOfferUserHandler(Bot bot, SteamID sid) : base(bot, sid) { }

        public override void OnNewTradeOffer(TradeOffer offer)
        {
            //receiving a trade offer 
            if (IsAdmin)
            {
                //parse inventories of bot and other partner
                //either with webapi or generic inventory
                //Bot.GetInventory();
                //Bot.GetOtherInventory(OtherSID);

                var myItems = offer.Items.GetMyItems();
                var theirItems = offer.Items.GetTheirItems();
                Log.Info("They want " + myItems.Count + " of my items.");
                Log.Info("And I will get " +  theirItems.Count + " of their items.");

                //do validation logic etc
                
                if (DummyValidation(myItems, theirItems))
                {
                    TradeOfferAcceptResponse acceptResp = offer.Accept();
                    if (acceptResp.Accepted)
                    {
                        Bot.AcceptAllMobileTradeConfirmations();
                        Log.Success("Accepted trade offer successfully : Trade ID: " + acceptResp.TradeId);
                    }
                }
                else
                {
                    // maybe we want different items or something

                    //offer.Items.AddMyItem(0, 0, 0);
                    //offer.Items.RemoveTheirItem(0, 0, 0);
                    if (offer.Items.NewVersion)
                    {
                        string newOfferId;
                        if (offer.CounterOffer(out newOfferId))
                        {
                            Bot.AcceptAllMobileTradeConfirmations();
                            Log.Success("Counter offered successfully : New Offer ID: " + newOfferId);
                        }
                    }
                }
            }
            else
            {
                //we don't know this user so we can decline
                if (offer.Decline())
                {
                    Log.Info("Declined trade offer : " + offer.TradeOfferId + " from untrusted user " + OtherSID.ConvertToUInt64());
                }
            }
        }

        public override void OnMessage(string message, EChatEntryType type)
        {
            if (IsAdmin)
            {

                //creating a new trade offer
                var offer = Bot.NewTradeOffer(OtherSID);
                offer.Items.AddMyItem(0, 0, 0);
                if (offer.Items.NewVersion)
                {
                    string newOfferId;
                    if (offer.Send(out newOfferId))
                    {
                        Bot.AcceptAllMobileTradeConfirmations();
                        Log.Success("Trade offer sent : Offer ID " + newOfferId);
                    }
                }

                //creating a new trade offer with token
                var offerWithToken = Bot.NewTradeOffer(OtherSID);

                //offer.Items.AddMyItem(0, 0, 0);
                if (offerWithToken.Items.NewVersion)
                {
                    string newOfferId;
                    // "token" should be replaced with the actual token from the other user
                    if (offerWithToken.SendWithToken(out newOfferId, "token"))
                    {
                        Bot.AcceptAllMobileTradeConfirmations();
                        Log.Success("Trade offer sent : Offer ID " + newOfferId);
                    }
                }
            }
        }

        // Format Command : exec 0 SteamID-token-Item1ID,Item2ID,Item3ID.....
        public override void OnBotCommand(string command)
        {
            Match match = Regex.Match(command, @"([0-9]+)-([A-Za-z0-9]+)-([0-9\,]+)$");

            // Getting User Steam ID
            SteamID OtherID = new SteamID();
            OtherID.SetFromUInt64(UInt64.Parse(match.Groups[1].ToString()));

            // Getting User token
            var token = match.Groups[2].ToString();

            //creating a new trade offer with token
            var offerWithToken = Bot.NewTradeOffer(OtherID);

            // Add item to offer
            string[] allItems = match.Groups[3].ToString().Split(',');

            foreach (var item in allItems)
            {
                long assetID = Int64.Parse(item);
                offerWithToken.Items.AddTheirItem(570, 2, assetID);
            }

            if (offerWithToken.Items.NewVersion)
            {
                string newOfferId;
                // "token" should be replaced with the actual token from the other user
                if (offerWithToken.SendWithToken(out newOfferId, token))
                {
                    //Bot.AcceptAllMobileTradeConfirmations();
                    Log.Success("Trade offer sent : Offer ID " + newOfferId);
                }
            }
        }

        public override bool OnGroupAdd() { return false; }

        public override bool OnFriendAdd() { return IsAdmin; }

        public override void OnFriendRemove() { }

        public override void OnLoginCompleted() { }

        public override bool OnTradeRequest() { return false; }

        public override void OnTradeError(string error) { }

        public override void OnTradeTimeout() { }

        public override void OnTradeSuccess() { }

        public override void OnTradeAwaitingConfirmation(long tradeOfferID) { }

        public override void OnTradeInit() { }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem) { }

        public override void OnTradeMessage(string message) { }

        public override void OnTradeReady(bool ready) { }

        public override void OnTradeAccept() { }

        private bool DummyValidation(List<TradeAsset> myAssets, List<TradeAsset> theirAssets)
        {
            ////compare items etc
            //if (myAssets.Count == theirAssets.Count)
            //{
            //    return true;
            //}
            //return false;
            return true;
        }
    }
}
