﻿namespace FreediverApp.BluetoothCommunication
{
    /**
     * This class holds the GUIDS of the Bluetooth services and characteristics that are  
     * advertised by the arduino.
     **/
    public static class BluetoothServiceData
    {
        public static string DIVE_SERVICE_ID = "19B10000-E8F3-537E-4F6C-D194768A2214";
        public static string DIVE_CHARACTERISTIC_ID = "19B10001-E8F2-537E-4F6C-D104768A1214";

        /**
         * These were used as an attempt to boost performance using one characteristic for 
         * each variable that needed to be transmitted from arduino to the app.
         **/
        public static string CHARACTERISTIC_ACCELERATOR_X = "094a4ab5-d789-477e-8b3a-fd5fee1971f9";
        public static string CHARACTERISTIC_ACCELERATOR_Y = "63e30608-53f1-48fb-a7f1-8363ed609a39";
        public static string CHARACTERISTIC_ACCELERATOR_Z = "82dea7b6-2091-4a8a-a788-fcaf0fba84e4";
        public static string CHARACTERISTIC_DEPTH = "70b263a4-faa9-4940-96a6-ce481cfd5803";
        public static string CHARACTERISTIC_DURATION = "21980bb5-887a-4a8f-9598-51bb19b41068";
        public static string CHARACTERISTIC_GYROSCOPE_X = "b6cf7ab8-525d-4e27-86e9-c4e7fe32223c";
        public static string CHARACTERISTIC_GYROSCOPE_Y = "c4fa00ce-5af9-40b7-b960-1658a8074320";
        public static string CHARACTERISTIC_GYROSCOPE_Z = "5475ba53-bcbd-43de-b410-c06ed3a50142";
        public static string CHARACTERISTIC_HEART_FREQ = "5fa8fde9-8717-45b8-8d7a-2b4da78b627a";
        public static string CHARACTERISTIC_HEART_VAR = "af2dc2ff-e565-4183-ae8a-a5bc018967c3";
        public static string CHARACTERISTIC_LUMINANCE = "4433820f-7709-4881-ad18-adc063ddc0a1";
        public static string CHARACTERISTIC_OXYGEN_SATURATION = "ab9db830-e5b5-4c12-8c9b-845386bf6dc8";
        public static string CHARACTERISTIC_REF_DIVE = "b8589826-9f39-445d-901e-699db1cad791";
        public static string CHARACTERISTIC_WATER_TEMP = "3473bdad-806f-419f-875a-0f2472e3fe53";

        public static string CHARACTERISTIC_ACK = "76e8ad5b-fe45-4871-a4c0-04962a35bbed";
        public static string CHARACTERISTIC_DATETIME = "496957e5-1b18-41e4-9028-800a68a476b4";         
    }
}