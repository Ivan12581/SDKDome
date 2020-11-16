package celia.sdk;

import android.app.AlarmManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;

import org.json.JSONException;
import org.json.JSONObject;

public class VitalitySender
{
    CeliaActivity mainActivity;
    public VitalitySender(CeliaActivity activity){

        mainActivity = activity;
    }
    public  void  ClearNotification()
    {
        AlarmManager am = (AlarmManager) mainActivity.getSystemService(Context.ALARM_SERVICE);
        Intent intent = new Intent(mainActivity, VitalityReceiver.class);
        intent.setAction("Celia_Vitality_0");
        PendingIntent sender = PendingIntent.getBroadcast(mainActivity, 0, intent, 0);
        am.cancel(sender);
        intent.setAction("Celia_Vitality_1");
        sender = PendingIntent.getBroadcast(mainActivity, 0, intent, 0);
        am.cancel(sender);
        intent.setAction("Celia_Vitality_2");
        sender = PendingIntent.getBroadcast(mainActivity, 0, intent, 0);
        am.cancel(sender);
    }

    public void RegisterNotification(String jsonStr)
    {
        try {
            JSONObject jsonObject = new JSONObject(jsonStr);
            int type = jsonObject.getInt("type");
            String title = jsonObject.getString("title");
            String text = jsonObject.getString("text");
            long timeStamp = jsonObject.getLong("timeStamp");
            System.out.println("Method called" + timeStamp);
            Intent intent = new Intent(mainActivity, VitalityReceiver.class);
            intent.setAction("Celia_Vitality_" + type);

            PendingIntent sender = PendingIntent.getBroadcast(mainActivity, 0, intent, 0);
            AlarmManager am = (AlarmManager) mainActivity.getSystemService(Context.ALARM_SERVICE);
            switch(type)
            {
                default:
                    break;
                case 0:
                    Constant.Notification_Title_Full = title;
                    Constant.Notification_Text_Full = text;
                    am.set(AlarmManager.RTC,(timeStamp * 1000),sender);
                    break;
                case 1:
                    Constant.Notification_Title_CollectMorning = title;
                    Constant.Notification_Text_CollectMorning = text;
                    am.setRepeating(AlarmManager.RTC,(timeStamp * 1000),AlarmManager.INTERVAL_DAY,sender);
                    break;
                case 2:
                    Constant.Notification_Title_CollectAfternoon = title;
                    Constant.Notification_Text_CollectAfternoon = text;
                    am.setRepeating(AlarmManager.RTC,(timeStamp * 1000),AlarmManager.INTERVAL_DAY,sender);
                    break;
            }


        } catch (JSONException e) {
            e.printStackTrace();
        }

    }
}
