package celia.sdk;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Build;

import androidx.core.app.NotificationCompat;

import com.xlycs.rastar.R;

public class VitalityReceiver extends BroadcastReceiver {
    private static int NOTIFICATION_FLAG = 101801;

    String id = "CeliaChannel";
    String name="Celia_Vitality";

    String GetText(int type)
    {
        switch(type)
        {
            default:
            case 0:
                return  "体力已满";
            case 1:
                return  "公主殿下，可以上线领取体力了呦";
            case 2:
                return  "公主殿下，可以上线领取体力了呦";
        }
    }

    String GetTitle(int type)
    {
        switch(type)
        {
            default:
            case 0:
                return  "体力溢出通知";
            case 1:
                return  "早餐时间到～";
            case 2:
                return  "下午茶时间到～";
        }
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        System.out.println("BroadCast Received");
        if (intent.getAction().equals("Celia_Vitality_0"))
        {
            ShotNotification(0,context,intent);
        }
        else if (intent.getAction().equals("Celia_Vitality_1"))
        {
            ShotNotification(1,context,intent);
        }
        else if (intent.getAction().equals("Celia_Vitality_2"))
        {
            ShotNotification(2,context,intent);
        }
    }

    void ShotNotification(int type,Context context, Intent intent)
    {
        NotificationManager manager = (NotificationManager) context
                .getSystemService(Context.NOTIFICATION_SERVICE);
        Notification notification = null;
        PendingIntent pendingIntent = PendingIntent.getActivity(context, 0,
                new Intent(context, CeliaActivity.class), 0);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel mChannel = new NotificationChannel(id, name, NotificationManager.IMPORTANCE_HIGH);
            manager.createNotificationChannel(mChannel);
            Notification.Builder notiBuilder = new Notification.Builder(context)
                    .setChannelId(id)
                    .setContentTitle(GetTitle(type))
                    .setContentText(GetText(type))
                    .setSmallIcon(com.xlycs.rastar.resources.R.drawable.notificationicon)
                    .setLargeIcon(BitmapFactory
                            .decodeResource(context.getResources(),R.mipmap.app_icon))
                    .setContentIntent(pendingIntent)
                    .setAutoCancel(true);
            notification = notiBuilder.build();
        } else {
            Notification.Builder notiBuilder = new Notification.Builder(context)
                    .setDefaults(Notification.DEFAULT_SOUND)
                    .setContentTitle(GetTitle(type))
                    .setContentText(GetText(type))
                    .setSmallIcon(com.xlycs.rastar.resources.R.drawable.notificationicon)
                    .setLargeIcon(BitmapFactory
                            .decodeResource(context.getResources(),R.mipmap.app_icon))
                    .setContentIntent(pendingIntent)
                    .setAutoCancel(true);
            notification = notiBuilder.build();
        }
        manager.notify(NOTIFICATION_FLAG + type, notification);
    }


}