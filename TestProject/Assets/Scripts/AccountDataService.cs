using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace celia.game
{
    public partial class Notif
    {
        public const string NAME_CHANGED = "NAME_CHANGED";
    }

    public class AccountDataService : BaseService<AccountDataService>
    {
        AccountData accountData;

        public void InitializeData(l2c_initialize data)
        {
            NetworkManager.gi.RegisterMsgHandler(LogicMsgID.LogicMsgL2CUpdPlayer, 
            new EventHandler<TcpClientEventArgs>(OnLogicMsgL2CUpdPlayer));
            
            this.accountData = new AccountData
            {
                vitality = data.Vitality,
                name = data.Name,
                maxNormal = data.Maxnomal,
                maxDifficult = data.Maxdiffcult,
            };
        }
        
        private void OnLogicMsgL2CUpdPlayer(object sender, TcpClientEventArgs e)
        {
            l2c_upd_player msg = l2c_upd_player.Parser.ParseFrom(e.msg);
            UpdateVitality(msg.Vitality);
            UpdateName(msg.Name);
            accountData.maxNormal = msg.Maxnomal;
            accountData.maxDifficult = msg.Maxdiffcult;
        }

        public AccountData getAccountData()
        {
            return accountData;
        }

        public void UpdateName(string value)
        {
            if (accountData.name != value)
            {
                accountData.name = value;
                Messenger.DispatchEvent(Notif.NAME_CHANGED);
            }
        }

        public void UpdateVitality(int value)
        {
          //  Messenger.DispatchEvent(Notif.VITALITY_CHANGED_BEFORE);
            accountData.vitality = value;
            Messenger.DispatchEvent(Notif.VITALITY_CHANGED);
        }

        public string nameKeeper;
        /*
        public void RequestSetUserName(string name, Action<bool> callback)
        {
            new GameSparks.Api.Requests.ChangeUserDetailsRequest()
            .SetDisplayName(name)
            .Send((res) =>
            {
                if (res.HasErrors)
                {
                    Debug.Log(res.Errors.JSON);
                }
                else
                {
                    nameKeeper = name;
                }
                callback?.Invoke(!res.HasErrors);
            });
        }
        */
    }

    public class AccountData
    {
        public int vitality;
        public int lastRecoverVitalityTime;
        public string name;
        public int maxNormal;
        public int maxDifficult;

        public int restRcvTime;
    }

}