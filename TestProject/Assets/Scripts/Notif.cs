using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace celia.game
{
    public partial class Notif
    {
        #region General
        public const string INHOUSE_LOGIN_INIT_DATA_COMPLETED = "INHOUSE_LOGIN_INIT_DATA_COMPLETED";
        public const string LOG_IN = "LOG_IN";
        public const string LOG_OUT = "LOG_OUT";
        public const string NO_NAME_LOG_IN = "NO_NAME_LOG_IN";
        public const string LOGIN_FAIL = "LOGIN_FAIL";
        // public const string GS_LOGIN_INIT_DATA_COMPLETED = "GS_LOGIN_INIT_DATA_COMPLETED";
        public const string SUCCESSFULLY_UPDATE_CLOTHES_DATA = "SUCCESSFULLY_UPDATE_CLOTHES_DATA";

        public const string PARSE_CONFIG = "PARSE_CONFIG";
        /// <summary>
        /// Live2D模型重新拍照完成事件，在Live2DCapture完成拍照后会派发一次。
        /// </summary>
        public const string Live2D_SPRITES_UPDATED = "Live2D_SPRITES_UPDATED";

        public const string GOLD_UPDATE = "GOLD_UPDATE";
        public const string DIAMOND_UPDATE = "DIAMOND_UPDATE";

        public const string SWITCH_SCENE = "SWITCH_SCENE";
        /// <summary>
        /// 登录认证服成功
        /// </summary>
        public const string LoginAuth_SUCCESS = "LoginAuth_SUCCESS";
        /// <summary>
        /// 登录认证服失败
        /// </summary>
        public const string LoginAuth_FAIL = "LoginAuth_FAIL";
        /// <summary>
        /// 游客账号绑定结果通知
        /// </summary>
        public const string AuthTouristBind_INFO = "AuthTouristBind_INFO";
        #endregion

        #region PopupMessage 
        public const string POPUPMESSAGE_TIP = "POPUPMESSAGE_TIP";

        #endregion

        #region VIEW
        public const string VIEW_OPEN = "VIEW_OPEN";
        public const string VIEW_CLOSE = "VIEW_CLOSE";
        public const string MAINVIEW_HIDE = "MAINVIEW_HIDE";
        public const string COMPONENT_CLOSE = "COMPONENT_CLOSE";

        #endregion

        #region Tab
        public const string ON_TAB_TOGGLE = "ON_TAB_TOGGLE";
        public const string ON_WECHAT_TAB_TOGGLE = "ON_WECHAT_TAB_TOGGLE";
        #endregion

        #region UI
        public const string CHECK_NEW_MESSAGE = "CHECK_NEW_MESSAGE";
        #endregion

        #region Loading
        public const string LOAD_CONFIG_COMPLETED = "LOAD_CONFIG_COMPLETED";
        #endregion

        #region Bulletin
        public const string NEW_BULLETIN_DATA = "NEW_BULLETIN_DATA";
        #endregion

        #region Wechat
        public const string WECHAT_ITEM_PRESSED = "WECHAT_ITEM_PRESSED";
        public const string SHOW_ARTICLE_DETAILED = "SHOW_ARTICLE_DETAILED";
        public const string VOICE_CALL_OPEN = "VOICE_CALL_OPEN";
        public const string VOICE_CALL_CLOSE = "VOICE_CALL_CLOSE";
        public const string MSG_DETAILED_NEED_SCROLL_TO_BOTTOM = "MSG_DETAILED_NEED_SCROLL_TO_BOTTOM";
        public const string WECHAT_REDDOT = "WECHAT_REDDOT";
        #endregion

        #region Battle

        public const string BATTLE_ISOVER = "BATTLE_ISOVER";
        public const string CLOSE_BATTLE_RESULT_VIEW = "CLOSE_BATTLE_RESULT_VIEW";
        public const string REBATTLE = "REBATTLE";
        public const string UPDATE_HP = "UPDATE_HP";
        public const string UPDATE_ROLE_HP = "UPDATE_ROLE_HP";
        //public const string UPDATE_CHECK_HP = "UPDATE_CHECK_HP";
        public const string UPDATE_POWER = "UPDATE_POWER";
        public const string ROUND_START = "ROUND_START";
        public const string ROUND_COMPLETED = "ROUND_COMPLETED";
        public const string RECALCULATE_AT = "RECALCULATE_AT";
        public const string CUT_IN_COMPLETED = "CUT_IN_COMPLETED";
        public const string CHECK_BATTLE_IS_OVER = "CHECK_BATTLE_IS_OVER";
        public const string ADD_POWER = "ADD_POWER";
        public const string DO_NEXT_ACTION = "DO_NEXT_ACTION";
        public const string MOVE_COMPLETED = "MOVE_COMPLETED";
        public const string POWER_FULL_HIT = "POWER_FULL_HIT";
        public const string SING_START = "SING_START";
        public const string PS_ACTION_STOP = "PS_ACTION_STOP";
        public const string ROLE_DIE = "ROLE_DIE";
        public const string SING_OVER = "SING_OVER";
        public const string UPDATE_ACTIONS_BY_SPEED = "UPDATE_ACTIONS_BY_SPEED";
        public const string RESET_ORDER_BY_STUN = "RESET_ORDER_BY_STUN";
        public const string ADD_NEW_ACTION_ORDER = "ADD_NEW_ACTION_ORDER";
        public const string AUTO_SKILL_COMPLETED = "AUTO_SKILL_COMPLETED";
        public const string ATTACK_BACK_OVER = "ATTACK_BACK_OVER";
        public const string COMBINED_START = "COMBINED_START";

        public const string UI_ICE_SHOW = "UI_ICE_SHOW";
        public const string UI_ICE_CLOSE = "UI_ICE_CLOSE";

        public const string ORB_ABSORB = "ORB_ABSORB";

        public const string BATTLE_DIALOG_DIE = "BATTLE_DIALOG";
        public const string BATTLE_DIALOG_INTERVENE = "BATTLE_DIALOG_INTERVENE";
        public const string BATTLE_DIALOG_SWITCH = "BATTLE_DIALOG_SWITCH";

        public const string CHECK_BGM_CONDITION = "CHECK_BGM_CONDITION";

        //Buff
        public const string BUFF_CHECK_POISON_ACTION_DAMAGE = "BUFF_CHECK_POISON_ACTION_DAMAGE";
        public const string BUFF_CHECK_POISON_BEATTACK_DAMAGE = "BUFF_CHECK_POISON_BEATTACK_DAMAGE";
        public const string BUFF_CHECK_CRYSTAL_STATE = "BUFF_CHECK_CRYSTAL_STATE";
        public const string BUFF_REMOVE_CRYSTAL_STATE = "BUFF_REMOVE_CRYSTAL_STATE";
        public const string BUFF_BURN_CHECK_ACTION_DAMAGE = "BUFF_BURN_BEATTACK_DAMAGE";

        public const string BUFF_EXECUTE_SKILLED = "BUFF_EXECUTE_SKILLED";

        //Buff end
        public const string EF_CUT_BULLET = "EF_CUT_BULLET";
        //EF

        //EF end

        //Guide
        /// <summary>
        /// 教程26
        /// </summary>
        public const string GUIDE_BATTLE_START = "GUIDE_BATTLE_START";
        /// <summary>
        /// 教程27
        /// </summary>
        public const string GUIDE_ATTACK = "GUIDE_ATTACK";
        /// <summary>
        /// 教程28
        /// </summary>
        public const string GUIDE_POWER_BALL = "GUIDE_POWER_BALL";
        /// <summary>
        /// 教程29
        /// </summary>
        public const string GUIDE_POWER_FULL = "GUIDE_POWER_FULL";
        /// <summary>
        /// 教程30
        /// </summary>
        public const string GUIDE_COMBINED_SKILL = "GUIDE_COMBINED_SKILL";
        /// <summary>
        /// 教程31
        /// </summary>
        public const string GUIDE_AT_SYSTEM = "GUIDE_AT_SYSTEM";

        //Guide end

        //============================================================================
        public const string BATTLE_ROUND_CHANGED = "BATTLE_ROUND_CHANGED";
        public const string BATTLE_SPEED_CHANGED = "BATTLE_SPEED_CHANGE";
        public const string BATTLE_TIME_SWITCH = "BATTLE_TIME_SWITCH";
        //============================================================================
        #endregion

        #region LeveLMap

        public const string LEVELMAP_ENTERPREBATTLE = "LEVELMAP_ENTERPREBATTLE";
        public const string LEVELMAP_C2S_GETLEVELMAPSERDATAS = "LEVELMAP_C2S_GETLEVELMAPSERDATAS";
        public const string LEVELMAP_S2C_GETLEVELMAPSERDATAS = "LEVELMAP_S2C_GETLEVELMAPSERDATAS";
        public const string LEVELMAP_CHANGECHAPTER = "LEVELMAP_CHANGECHAPTER";

        #endregion


        #region   Story

        public const string STORY_STARTCURACTION = "STORY_STARTCURACTION";
        public const string STORY_CONTINUE = "STORY_CONTINUE";
        public const string STORY_GETNEXTSTORYACTIONEVENT = "STORY_GETNEXTSTORYACTIONEVENT";
        public const string STORY_STARTEFFECT = "STORY_STARTEFFECT";

        public const string STORY_EXIT = "STORY_EXIT";

        public const string STORY_EXITMINIGAME = "STORY_EXITMINIGAME";

        #endregion



        #region  TMPTEST
        public static string REDUCE_VITALITY = "REDUCE_VITALITY";

        #endregion



        #region  Account
        public const string RECOVER_VITALITY = "RECOVER_VITALITY";
        public const string VITALITY_CHANGED = "VITALITY_CHANGED";
        public const string VITALITY_CHANGED_BEFORE = "VITALITY_CHANGED_BEFORE";
        public const string START_VITALITY_AUTO_RECOVER_JOB = "START_VITALITY_AUTO_RECOVER_JOB";

        #endregion

        #region StageMap
        public const string STAGE_MAP_VIEW_SHOWED = "STAGE_MAP_VIEW_SHOWED";
        public const string UPDATE_STAGE_NODE = "UPDATE_STAGE_NODE";
        public const string PASSED_STAGE_NODE = "PASSED_STAGE_NODE";
        public const string INITIALIZED_STAGE_DATA = "INITIALIZED_STAGE_DATA";
        public const string ON_CLICK_PREBATTLE_RETURN = "ON_CLICK_PREBATTLE_RETURN";
        #endregion

        #region Engagement

        public const string ENGAGEMENT_GETUNLOCKLIST = "ENGAGEMENT_GETUNLOCKLIST";
        public const string ENGAGEMENT_SCENEDATALOCKTIP = "ENGAGEMENT_SCENEDATALOCKTIP";
        public const string ENGAGEMENT_REQUESSTORYACCOUNTREWARD = "ENGAGEMENT_REQUESSTORYACCOUNTREWARD";
        public const string ENGAGEMENT_UPDATEUNLOCK = "ENGAGEMENT_UPDATEUNLOCK";
        public const string ENGAGEMENT_REQUESSENDGIFTSLIST = "ENGAGEMENT_REQUESSENDGIFTSLIST";
        public const string ENGAGEMENT_SENDGIFT = "ENGAGEMENT_SENDGIFT";
        public const string ENGAGEMENT_ENTERSENDGIFT = "ENGAGEMENT_ENTERSENDGIFT";
        public const string ENGAGEMENT_SELECT = "ENGAGEMENT_SELECT";
        public const string ENGAGEMENT_AFTERUNLOCK = "ENGAGEMENT_AFTERUNLOCK";
        public const string ENGAGEMENT_UPDATE = "ENGAGEMENT_UPDATE";

        #endregion

        #region MiniGameClothChange
        public const string MINIGAME_CLOTH_CHANGE = "MINIGAME_CLOTH_CHANGE";
        public const string MINIGAME_CLOTHCHANGE_FINISH = "MINIGAME_CLOTHCHANGE_FINISH";
        public const string MINIGAME_CLOTHCHANGE_FINISHNORESULT = "MINIGAME_CLOTHCHANGE_FINISHNORESULT";
        #endregion

        #region FishingGame
        public const string FISHING_LEVEL_UPDATED = "FISHING_LEVEL_UPDATED";
        public const string FISHING_EXP_UPDATED = "FISHING_EXP_UPDATED";
        public const string FISHING_ROD_UPDATED = "FISHING_ROD_UPDATED";
        public const string FISHING_BAIT_UPDATED = "FISHING_BAIT_UPDATED";
        public const string FISHING_ACCESSORY_UPDATED = "FISHING_ACCESSORY_UPDATED";
        public const string FISHING_SELECT_ROD = "FISHING_SELECT_ROD";
        public const string FISHING_SELECT_BAIT = "FISHING_SELECT_BAIT";
        public const string FISHING_SELECT_ACCESSORY = "FISHING_SELECT_ACCESSORY";
        public const string FISHING_SINGLE_GAME_START = "FISHING_SINGLE_GAME_START";
        public const string FISHING_SINGLE_GAME_QUIT = "FISHING_SINGLE_GAME_QUIT";
        public const string FISHING_SINGLE_GAME_SUCCESS = "FISHING_SINGLE_GAME_SUCCESS";
        public const string FISHING_SINGLE_GAME_FAIL = "FISHING_SINGLE_GAME_FAIL";
        public const string FISHING_RANDOM_STAGE_GAME_SUCCESS = "FISHING_RANDOM_STAGE_GAME_SUCCESS";
        public const string FISHING_RANDOM_STAGE_GAME_FAIL = "FISHING_RANDOM_STAGE_GAME_FAIL";
        public const string FISHING_ACTIVITY_END = "FISHING_ACTIVITY_END";
        public const string FISHING_ACTIVITY_TASK_UPD = "FISHING_ACTIVITY_TASK_UPD";
        public const string FISHING_ACTIVITY_ROUND_UPD = "FISHING_ACTIVITY_ROUND_UPD";
        #endregion

        #region NewBird

        public const string NEWBIRD_STARTGUIDE = "NEWBIRD_STARTGUIDE";

        #endregion


        #region ShoppingMall

        public const string SHOPPINGMALL_CLOTHINGTIP = "SHOPPINGMALL_CLOTHINGTIP";
        public const string SHOPPINGMALL_COMMONTIP = "SHOPPINGMALL_COMMONTIP";

        public const string SHOPPINGMALL_REQUESTSHOPPINGSUPPLYITEMS = "SHOPPINGMALL_REQUESTSHOPPINGSUPPLYITEMS";
        public const string SHOPPINGMALL_BUYONESUPPLYITEM_SUCCESS = "SHOPPINGMALL_BUYONESUPPLYITEM_SUCCESS";

        public const string SHOPPINGMALL_CLICKGIFTBAGITEM = "SHOPPINGMALL_CLICKGIFTBAGITEM";
        public const string SHOPPINGMALL_BUYONEGIFTBAG_UPDATE = "SHOPPINGMALL_BUYONEGIFTBAG_UPDATE";
        public const string SHOPPINGMALL_GIFTBAG_INIT = "SHOPPINGMALL_GIFTBAG_INIT";

        public const string SHOPPINGMALL_STOREGOODS_UPDATE = "SHOPPINGMALL_STOREGOODS_UPDATE";
        public const string SHOPPINGMALL_BUYPUBLICOPINION_SUCCESS = "SHOPPINGMALL_BUYPUBLICOPINION_SUCCESS";

        public const string SHOPPINGMALL_REQUESTSHOPPINGACTIVITYITEMS = "SHOPPINGMALL_REQUESTSHOPPINGACTIVITYITEMS";

        #endregion


        #region ChangeUIEffect

        public const string CHANGEUIEFFECT_STARTEFFECT = "CHANGEUIEFFECT_STARTEFFECT";


        #endregion

        #region Mail
        public const string MAIL_LIST_UPDATED = "MAIL_LIST_UPDATED";
        public const string MAIL_REDDOT_STATE_CHANGE = "MAIL_REDDOT_STATE_CHANGE";
        public const string MAIL_SINGLE_UPDATE = "MAIL_SINGLE_UPDATE";
        #endregion

        #region Clothing
        public const string SPECIAL_CLOTH_CHANGED = "SPECIAL_CLOTH_CHANGED";
        #endregion

        #region IndividualTower
        public const string UPDATE_TOWER_LEVEL = "UPDATE_TOWER_LEVEL";
        public const string UPDATE_ROLE_PASSED = "UPDATE_ROLE_PASSED";
        public const string UPDATE_TOWER_REWARDS = "UPDATE_TOWER_REWARDS";
        public const string REFRESH_TOWER_DATA = "REFRESH_TOWER_DATA";
        #endregion


        #region PopupView

        public const string POPUPVEIW_SHOW = "POPUPVEIW_SHOW";
        public const string POPUPVEIW_SET = "POPUPVEIW_SET";
        public const string POPUPVEIW_CLOSE = "POPUPVEIW_CLOSE";

        #endregion

        #region Encyclopedia

        public const string EO_REFRESH_NEW = "EO_REFRESH_NEW";

        #endregion


        #region player guide
        public const string PLAYER_GUIDE_FORCE_CLICKED = "PLAYER_GUIDE_FORCE_CLICKED";

        public const string PLAYER_GUIDE_UI_ANIME_FINISH = "PLAYER_GUIDE_UI_ANIME_FINISH";
        #endregion


        #region LZR_Guide    

        public const string PLAYER_GUIDE_LZR = "PLAYER_GUIDE_LZR";

        #endregion

        #region Explore    
        public const string CLICK_CHOOSE_CARD = "CLICK_CHOOSE_CARD";
        public const string REFRESH_EXPLORE_CARDS = "REFRESH_EXPLORE_CARDS";

        public const string UPDATE_EXPLORE_TASK = "UPDATE_EXPLORE_TASK";
        public const string SWITCH_EXPLORE_RED_DOT = "SWITCH_EXPLORE_RED_DOT";
        #endregion


        #region Achievement & Mission
        public const string ACHIEVEMENT_RED_DOT = "ACHIEVEMENT_RED_DOT";
        public const string ACHIEVEMENT_STAGE_RED_DOT = "ACHIEVEMENT_STAGE_RED_DOT";
        public const string MISSION_RED_DOT = "MISSION_RED_DOT";


        public const string MISSION_UPDATE = "MISSION_UPDATE";
        public const string ACHIEVEMENT_UPDATE = "ACHIEVEMENT_UPDATE";
        #endregion
        #region CollectionView
        public const string COLLECTION_RED_DOT = "COLLECTION_RED_DOT";
        public const string COLLECTION_SINGLE_ITEM_BTN_CLICK = "COLLECTION_SINGLE_ITEM_BTN_CLICK";
        public const string COLLECTION_SINGLE_SUIT_BTN_CLICK = "COLLECTION_SINGLE_SUIT_BTN_CLICK";
        public const string COLLECTION_SINGLE_OF_MULTI_SUIT_BTN_CLICK = "COLLECTION_SINGLE_OF_MULTI_SUIT_BTN_CLICK";
        public const string COLLECTION_SHOW_REWARD_DETAIL_BTN_CLICK = "COLLECTION_SHOW_REWARD_DETAIL_BTN_CLICK";
        public const string COLLECTION_MULTI_SUIT_BTN_CLICK = "COLLECTION_MULTI_SUIT_BTN_CLICK";

        #endregion

        #region DailyStage
        public const string DAILY_STAGE_REMAIN_COUN_UPD = "DAILY_STAGE_REMAIN_COUN_UPD";
        #endregion

        #region
        public const string POOLUPD = "TICKETUPD";
        #endregion

        #region QIAFAN
        public const string PURCHASE_UPDATE = "PURCHASE_UPDATE";
        public const string TOTALCREDITSUPDATE = "TOTALCREDITSUPDATE";
        public const string FIRSTRECHARGEUPDATE = "FIRSTRECHARGEUPDATE";

        #endregion

        #region RandomStage

        public const string RANDOMSTAGE_UPDATE = "RANDOMSTAGE_UPDATE";
        public const string RANDOMSTAGE_NEWTIPCLOSE = "RANDOMSTAGE_NEWTIPCLOSE";
        public const string RANDOMSTAGE_NEWTIPOPEN = "RANDOMSTAGE_NEWTIPOPEN";

        #endregion

        #region Share
        public const string SHAREUPDATE = "SHAREUPDATE";
        public const string ENDSHARE = "ENDSHARE";

        #endregion

        #region friend
        public const string FRIENDREMOVE = "FRIENDREMOVE";
        public const string FRIENDUPDATE = "FRIENDUPDATE";
        public const string APPLYREMOVE = "APPLYREMOVE";
        public const string APPLYUPDATE = "APPLYUPDATE";
        public const string FINDFRIEND = "FINDFRIEND";
        public const string FRIENDREDDOTUPDATE = "FRIENDREDDOTUPDATE";
        #endregion

        #region ChapterGiftBag

        public const string CHAPTERGIFTBAG_UPDATE = "CHAPTERGIFTBAG_UPDATE";


        #endregion

        #region Backpack
        public const string BACKPACK_CLOTH_BTN_CLICK = "BACKPACK_CLOTH_BTN_CLICK";
        public const string BACKPACK_SHOW_SELECT_PANEL = "BACKPACK_SHOW_SELECT_PANEL";
        public const string BACKPACK_SELECT_PANEL_ITEM = "BACKPACK_SELECT_PANEL_ITEM";
        #endregion

        #region PublicOpinion

        public const string PUBLICOPINION_UPDATECOUNTRYNATIVE = "PUBLICOPINION_UPDATECOUNTRYNATIVE";
        public const string PUBLICOPINION_UPDATEACTIVITYINFOS = "PUBLICOPINION_UPDATEACTIVITYINFOS";
        public const string PUBLICOPINION_UPDATESHOP = "PUBLICOPINION_UPDATESHOP";
        public const string PUBLICOPINION_REFRESH = "PUBLICOPINION_REFRESH";

        #endregion

        #region GiftBagCollection

        public const string GIFTBAGCOLLECTION_SETINFO = "GIFTBAGCOLLECTION_SETINFO";
        public const string GIFTBAGCOLLECTION_AFTERBUY = "GIFTBAGCOLLECTION_AFTERBUY";
        public const string GIFTBAGCOLLECTION_UPDATE = "GIFTBAGCOLLECTION_UPDATE";

        #endregion

        #region
        public const string MONTHCARDUPD = "MONTHCARDUPD";
        #endregion

        #region Acticity
        public const string ACTIVITY_INFOS_UPDATE = "ACTIVITY_INFOS_UPDATE";
        public const string ACTIVITY_INFO_REFRESH = "ACTIVITY_INFO_REFRESH";
        #endregion
    }
}
