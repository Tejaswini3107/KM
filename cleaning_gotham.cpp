#include <LiquidCrystal.h>
#include <SoftwareSerial.h>

// ═══ PIN DEFINITIONS ═══
#define ULTRASONIC_TRIG   2
#define ULTRASONIC_ECHO   3
#define WATER_POWER_PIN   4
#define RGB_RED           5
#define RGB_GREEN         6
#define RGB_BLUE          7
#define BUZZER_PIN        8
#define BUTTON_PIN        9
#define ESP_RX            10
#define ESP_TX            13
#define WATER_POT_PIN     A0
#define LDR_PIN           A5

// ═══ LCD ═══
#define LCD_RS            12
#define LCD_EN            11
#define LCD_D4            A1
#define LCD_D5            A2
#define LCD_D6            A3
#define LCD_D7            A4

// ═══ WIFI CONFIG ═══
#define WIFI_SSID         "Jeet"
#define WIFI_PASS         "333333334"
#define SERVER_IP         "web-app-production-fdb9.up.railway.app"
#define SERVER_PORT       80
#define API_KEY           "424242"

// ═══ THRESHOLDS ═══
#define BIN_HEIGHT_CM     30
#define FILL_MEDIUM       70
#define FILL_HIGH         90
#define LIGHT_DIM         200
#define LIGHT_BRIGHT      600
#define WATER_DRY         100
#define WATER_DAMP        300

// ═══ TIMING ═══
#define SENSOR_INTERVAL   500UL
#define WIFI_INTERVAL     10000UL
#define WIFI_CHECK        30000UL

LiquidCrystal lcd(LCD_RS, LCD_EN, LCD_D4, LCD_D5, LCD_D6, LCD_D7);
SoftwareSerial espSerial(ESP_RX, ESP_TX);

// ═══ SENSOR DATA ═══
int  bin1_fillPercent = 0;
int  bin2_lightLevel  = 0;
int  bin3_waterLevel  = 0;

byte bin1_state = 0;
byte bin2_state = 1;
byte bin3_state = 0;

const char B1_EMPTY[] PROGMEM = "EMPTY";
const char B1_HALF[]  PROGMEM = "HALF";
const char B1_FULL[]  PROGMEM = "FULL";
const char B2_BRIGHT[]PROGMEM = "BRIGHT";
const char B2_DIM[]   PROGMEM = "DIM";
const char B2_DARK[]  PROGMEM = "DARK";
const char B3_DRY[]   PROGMEM = "DRY";
const char B3_DAMP[]  PROGMEM = "DAMP";
const char B3_FLOOD[] PROGMEM = "FLOOD!";

char stateBuf[8];
char sharedBuf[160];

const char* getBin1State() {
  switch(bin1_state) {
    case 0: strcpy_P(stateBuf, B1_EMPTY); break;
    case 1: strcpy_P(stateBuf, B1_HALF);  break;
    case 2: strcpy_P(stateBuf, B1_FULL);  break;
  }
  return stateBuf;
}
const char* getBin2State() {
  switch(bin2_state) {
    case 0: strcpy_P(stateBuf, B2_BRIGHT); break;
    case 1: strcpy_P(stateBuf, B2_DIM);    break;
    case 2: strcpy_P(stateBuf, B2_DARK);   break;
  }
  return stateBuf;
}
const char* getBin3State() {
  switch(bin3_state) {
    case 0: strcpy_P(stateBuf, B3_DRY);   break;
    case 1: strcpy_P(stateBuf, B3_DAMP);  break;
    case 2: strcpy_P(stateBuf, B3_FLOOD); break;
  }
  return stateBuf;
}

// ═══ BUTTON ═══
int  buttonCount     = 0;
int  lastButtonCount = -1;
bool lastBtnState    = LOW;

// ═══ TIMING ═══
unsigned long lastSensorUpdate = 0;
unsigned long lastWifiSend     = 0;
unsigned long lastWifiCheck    = 0;

// ═══ LCD CUSTOM CHARS ═══
byte fullBlock[8] = {0b11111,0b11111,0b11111,0b11111,0b11111,0b11111,0b11111,0b11111};
byte lightIcon[8] = {0b00100,0b10101,0b01110,0b11111,0b11111,0b01110,0b10101,0b00100};
byte waterIcon[8] = {0b00100,0b00100,0b01110,0b01110,0b11111,0b11111,0b11111,0b01110};
byte wifiIcon[8]  = {0b00000,0b01110,0b10001,0b00100,0b01010,0b00000,0b00100,0b00000};


//  RGB LED HELPERS

void setRGBColor(bool r, bool g, bool b) {
  digitalWrite(RGB_RED,   r ? HIGH : LOW);
  digitalWrite(RGB_GREEN, g ? HIGH : LOW);
  digitalWrite(RGB_BLUE,  b ? HIGH : LOW);
}
void showRed()    { setRGBColor(true,  false, false); }
void showGreen()  { setRGBColor(false, true,  false); }
void showYellow() { setRGBColor(true,  true,  false); }
void showBlue()   { setRGBColor(false, false, true);  }
void rgbOff()     { setRGBColor(false, false, false); }
void setLEDsForBin(byte state) {
  if      (state == 2) showRed();
  else if (state == 1) showYellow();
  else                 showGreen();
}


//  BUTTON CHECK

void checkButton() {
  bool btnNow = digitalRead(BUTTON_PIN);
  if (btnNow == HIGH && lastBtnState == LOW) {
    digitalWrite(BUZZER_PIN, HIGH);
    delay(50);
    digitalWrite(BUZZER_PIN, LOW);
    buttonCount++;
    if (buttonCount > 3) buttonCount = 0;
    if (buttonCount == 0) { runStartupTest(); lastButtonCount = 0; }
    else lastButtonCount = -1;
  }
  lastBtnState = btnNow;
}


//  SMART DELAY

void smartDelay(unsigned long ms) {
  unsigned long start = millis();
  while (millis() - start < ms) {
    checkButton();
    delayMicroseconds(500);
  }
}


//  ESP8266 CORE

void flushESP() {
  smartDelay(80);
  while (espSerial.available()) espSerial.read();
}

bool waitFor(const char* expected, unsigned long timeoutMs) {
  unsigned long start = millis();
  byte idx = 0;
  memset(sharedBuf, 0, sizeof(sharedBuf));
  while (millis() - start < timeoutMs) {
    checkButton();
    while (espSerial.available()) {
      char c = (char)espSerial.read();
      if (idx < sizeof(sharedBuf) - 1) sharedBuf[idx++] = c;
      if (strstr(sharedBuf, expected)) return true;
    }
  }
  return false;
}

bool sendAT(const __FlashStringHelper* cmd, const char* expected, unsigned long ms) {
  flushESP();
  espSerial.println(cmd);
  return waitFor(expected, ms);
}

bool sendATstr(const char* cmd, const char* expected, unsigned long ms) {
  flushESP();
  espSerial.println(cmd);
  return waitFor(expected, ms);
}


//  HELLO HANDSHAKE — ping Railway server

bool pingServer() {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print(F("Pinging server.."));
  lcd.setCursor(0, 1);
  lcd.print(F("Railway cloud..."));

  snprintf(sharedBuf, sizeof(sharedBuf),
    "AT+CIPSTART=\"TCP\",\"%s\",%d", SERVER_IP, SERVER_PORT);

  if (!sendATstr(sharedBuf, "CONNECT", 8000)) {
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print(F("WiFi OK         "));
    lcd.setCursor(0, 1);
    lcd.print(F("Server offline! "));
    showYellow();
    smartDelay(2000);
    rgbOff();
    return false;
  }

  snprintf(sharedBuf, sizeof(sharedBuf),
    "GET /api/hello HTTP/1.0\r\nHost: %s\r\n\r\n",
    SERVER_IP);
  byte helloLen = strlen(sharedBuf);

  char sendCmd[20];
  snprintf(sendCmd, sizeof(sendCmd), "AT+CIPSEND=%d", helloLen);

  if (!sendATstr(sendCmd, ">", 3000)) {
    sendAT(F("AT+CIPCLOSE"), "OK", 1000);
    return false;
  }

  flushESP();
  espSerial.print(sharedBuf);

  memset(sharedBuf, 0, sizeof(sharedBuf));
  byte idx = 0;
  unsigned long start = millis();
  while (millis() - start < 5000) {
    checkButton();
    while (espSerial.available()) {
      char c = (char)espSerial.read();
      if (idx < sizeof(sharedBuf) - 1) sharedBuf[idx++] = c;
    }
  }
  sendAT(F("AT+CIPCLOSE"), "OK", 2000);

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print(F("Server says:    "));
  lcd.setCursor(0, 1);

  char* msgPtr = strstr(sharedBuf, "msg");
  if (msgPtr) {
    msgPtr += 6;
    char preview[17];
    strncpy(preview, msgPtr, 16);
    preview[16] = '\0';
    for (byte i = 0; i < 16; i++) {
      if (preview[i] == '"' || preview[i] == '}') { preview[i] = '\0'; break; }
    }
    lcd.print(preview);
  } else {
    lcd.print(F("Server OK!      "));
  }

  showGreen();
  smartDelay(2500);
  rgbOff();
  return true;
}


//  WIFI CONNECT — blocking until connected

void connectWiFiBlocking() {
  byte attempt = 0;

  while (true) {
    attempt++;
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print(F("WiFi connecting "));
    lcd.setCursor(0, 1);
    lcd.print(F("Try #"));
    lcd.print(attempt);
    lcd.print(F(" "));
    lcd.print(F(WIFI_SSID));
    showBlue();

    espSerial.println(F("AT+RST"));
    smartDelay(3000);
    flushESP();

    if (!sendAT(F("AT"), "OK", 3000)) {
      rgbOff(); smartDelay(2000); continue;
    }

    sendAT(F("ATE0"), "OK", 2000);
    sendAT(F("AT+RFPOWER=0"), "OK", 2000);  // reduce TX power → less current spike
    sendAT(F("AT+CWMODE=1"), "OK", 3000);
    smartDelay(500);

    snprintf(sharedBuf, sizeof(sharedBuf),
      "AT+CWJAP=\"%s\",\"%s\"", WIFI_SSID, WIFI_PASS);

    lcd.setCursor(0, 1);
    lcd.print(F("Joining...      "));

    if (sendATstr(sharedBuf, "WIFI GOT IP", 20000)) {
      lcd.clear();
      lcd.setCursor(0, 0);
      lcd.print(F("WiFi connected! "));
      lcd.setCursor(0, 1);
      lcd.print(F("Reaching Railway"));
      showGreen();
      smartDelay(1000);
      rgbOff();

      pingServer();
      return;
    }

    lcd.setCursor(0, 1);
    lcd.print(F("Failed, retry..."));
    showRed();
    smartDelay(2000);
    rgbOff();
  }
}


//  WIFI HEALTH CHECK

bool checkWiFiAlive() {
  char check[] = "AT+CWJAP?";
  return sendATstr(check, WIFI_SSID, 3000);
}


//  WIFI LOST — FREEZE and reconnect

void handleWiFiLost() {
  digitalWrite(BUZZER_PIN, LOW);
  rgbOff();

  while (true) {
    lcd.clear();
    lcd.setCursor(0, 0);
    lcd.print(F("WiFi lost!      "));
    lcd.setCursor(0, 1);
    lcd.print(F("Reconnecting... "));
    showRed(); smartDelay(500); rgbOff(); smartDelay(500);

    espSerial.println(F("AT+RST"));
    smartDelay(3000);
    flushESP();

    if (!sendAT(F("AT"), "OK", 3000)) continue;

    sendAT(F("ATE0"), "OK", 2000);
    sendAT(F("AT+RFPOWER=0"), "OK", 2000);  // low TX power on reconnect too
    sendAT(F("AT+CWMODE=1"), "OK", 3000);
    smartDelay(500);

    snprintf(sharedBuf, sizeof(sharedBuf),
      "AT+CWJAP=\"%s\",\"%s\"", WIFI_SSID, WIFI_PASS);

    if (sendATstr(sharedBuf, "WIFI GOT IP", 20000)) {
      lcd.clear();
      lcd.setCursor(0, 0);
      lcd.print(F("WiFi restored!  "));
      showGreen();
      smartDelay(1000);
      rgbOff();

      pingServer();

      // FIX: reset all timers after reconnect too
      lastWifiCheck  = millis();
      lastWifiSend   = millis();
      lastButtonCount = -1;
      lcd.clear();
      return;
    }
  }
}


//  SEND DATA TO RAILWAY SERVER

bool sendToServer() {
  sendAT(F("AT+CIPCLOSE"), "OK", 1000);

  snprintf(sharedBuf, sizeof(sharedBuf),
    "AT+CIPSTART=\"TCP\",\"%s\",%d", SERVER_IP, SERVER_PORT);

  if (!sendATstr(sharedBuf, "CONNECT", 8000)) return false;

  snprintf(sharedBuf, sizeof(sharedBuf),
    "GET /api/update?key=%s"
    "&b1=%d&s1=%s&b2=%d&s2=%s&b3=%d&s3=%s&al=%d"
    " HTTP/1.0\r\nHost: %s\r\n\r\n",
    API_KEY,
    bin1_fillPercent, getBin1State(),
    bin2_lightLevel,  getBin2State(),
    bin3_waterLevel,  getBin3State(),
    (bin1_state==2||bin3_state==2) ? 1 : 0,
    SERVER_IP);

  char sendCmd[20];
  snprintf(sendCmd, sizeof(sendCmd), "AT+CIPSEND=%d", strlen(sharedBuf));

  if (!sendATstr(sendCmd, ">", 3000)) {
    sendAT(F("AT+CIPCLOSE"), "OK", 1000);
    return false;
  }

  flushESP();
  espSerial.print(sharedBuf);
  smartDelay(5000);
  sendAT(F("AT+CIPCLOSE"), "OK", 2000);

  // Reset WiFi check timer — ESP is busy after TX,
  // don't run health check immediately after sending
  lastWifiCheck = millis();

  Serial.println(F("Data sent OK!"));
  return true;
}


//  SETUP

void setup() {
  Serial.begin(9600);
  espSerial.begin(9600);

  pinMode(ULTRASONIC_TRIG, OUTPUT);
  pinMode(ULTRASONIC_ECHO, INPUT);
  pinMode(WATER_POWER_PIN, OUTPUT);
  digitalWrite(WATER_POWER_PIN, LOW);
  pinMode(RGB_RED,   OUTPUT);
  pinMode(RGB_GREEN, OUTPUT);
  pinMode(RGB_BLUE,  OUTPUT);
  pinMode(BUZZER_PIN, OUTPUT);
  digitalWrite(BUZZER_PIN, LOW);
  pinMode(BUTTON_PIN, INPUT);

  lcd.begin(16, 2);
  lcd.createChar(0, fullBlock);
  lcd.createChar(1, lightIcon);
  lcd.createChar(2, waterIcon);
  lcd.createChar(3, wifiIcon);

  connectWiFiBlocking();
  runStartupTest();

  // ══ FIX: reset timers AFTER boot sequence completes ══
  // Without this, lastWifiCheck=0 causes health check to
  // fire immediately → thinks WiFi lost → infinite loop
  lastWifiCheck  = millis();
  lastWifiSend   = millis();
  lastSensorUpdate = millis();

  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print(F("Press btn > Bin1"));
}


//  MAIN LOOP

void loop() {

  checkButton();

  // WiFi health check every 15s
  if (millis() - lastWifiCheck >= WIFI_CHECK) {
    lastWifiCheck = millis();
    if (!checkWiFiAlive()) handleWiFiLost();
  }

  // Sensors every 500ms
  if (millis() - lastSensorUpdate >= SENSOR_INTERVAL) {
    lastSensorUpdate = millis();

    bin1_fillPercent = readUltrasonic();
    bin2_lightLevel  = analogRead(LDR_PIN);

    digitalWrite(WATER_POWER_PIN, HIGH);
    smartDelay(10);
    bin3_waterLevel = analogRead(WATER_POT_PIN);
    digitalWrite(WATER_POWER_PIN, LOW);

    if      (bin1_fillPercent >= FILL_HIGH)   bin1_state = 2;
    else if (bin1_fillPercent >= FILL_MEDIUM) bin1_state = 1;
    else                                       bin1_state = 0;

    if      (bin2_lightLevel < LIGHT_DIM)     bin2_state = 2;
    else if (bin2_lightLevel < LIGHT_BRIGHT)  bin2_state = 1;
    else                                       bin2_state = 0;

    if      (bin3_waterLevel >= WATER_DAMP)   bin3_state = 2;
    else if (bin3_waterLevel >= WATER_DRY)    bin3_state = 1;
    else                                       bin3_state = 0;

    digitalWrite(BUZZER_PIN,
      (bin1_state==2||bin3_state==2) ? HIGH : LOW);

    if (buttonCount != lastButtonCount) {
      lastButtonCount = buttonCount;
      lcd.clear();
    }

    if (buttonCount == 1) { showBin1(); setLEDsForBin(bin1_state); }
    if (buttonCount == 2) { showBin2(); setLEDsForBin(bin2_state); }
    if (buttonCount == 3) { showBin3(); setLEDsForBin(bin3_state); }

    if (buttonCount >= 1 && buttonCount <= 3) {
      lcd.setCursor(12, 0);
      lcd.write(byte(3));
      lcd.print(F("OK "));
    }

    Serial.print(F("B1:")); Serial.print(bin1_fillPercent);
    Serial.print(F("%|B2:")); Serial.print(bin2_lightLevel);
    Serial.print(F("|B3:")); Serial.println(bin3_waterLevel);
  }

  // Send to Railway every 10s
  if (millis() - lastWifiSend >= WIFI_INTERVAL) {
    lastWifiSend = millis();
    if (!sendToServer()) {
      Serial.println(F("Send failed!"));
    }
  }
}


//  LCD DISPLAY FUNCTIONS

void showBin1() {
  lcd.setCursor(0, 0);
  lcd.print(F("Bin1:"));
  lcd.setCursor(5, 0);
  if (bin1_fillPercent < 10)  lcd.print(F(" "));
  if (bin1_fillPercent < 100) lcd.print(F(" "));
  lcd.print(bin1_fillPercent);
  lcd.print(F("%  "));

  lcd.setCursor(0, 1);
  lcd.print(F("                "));
  lcd.setCursor(0, 1);
  const char* s = getBin1State();
  lcd.print(s);
  int p = strlen(s) + 1;
  lcd.print(F(" "));
  int bars = map(bin1_fillPercent, 0, 100, 0, 16 - p);
  for (int i = 0; i < 16 - p; i++) {
    if (i < bars) lcd.write(byte(0));
    else          lcd.print(F("-"));
  }
}

void showBin2() {
  lcd.setCursor(0, 0);
  lcd.print(F("Bin2:"));
  lcd.setCursor(5, 0);
  lcd.print(bin2_lightLevel);
  lcd.print(F("   "));
  lcd.setCursor(0, 1);
  if (bin2_state == 2) {
    lcd.write(byte(1)); lcd.print(F(" DARK       "));
    lcd.write(byte(1)); lcd.print(F(" "));
  } else if (bin2_state == 1) {
    lcd.print(F("Status: DIM     "));
  } else {
    lcd.print(F("Status: BRIGHT  "));
  }
}

void showBin3() {
  lcd.setCursor(0, 0);
  lcd.print(F("Bin3:Lvl "));
  lcd.print(bin3_waterLevel);
  lcd.print(F("   "));
  lcd.setCursor(0, 1);
  if (bin3_state == 2) {
    lcd.write(byte(2)); lcd.print(F(" WATER ALERT!"));
    lcd.write(byte(2));
  } else if (bin3_state == 1) {
    lcd.print(F("Status: DAMP    "));
  } else {
    lcd.print(F("Status: DRY     "));
  }
}


//  ULTRASONIC SENSOR

int readUltrasonic() {
  digitalWrite(ULTRASONIC_TRIG, LOW);
  delayMicroseconds(2);
  digitalWrite(ULTRASONIC_TRIG, HIGH);
  delayMicroseconds(10);
  digitalWrite(ULTRASONIC_TRIG, LOW);
  long dur = pulseIn(ULTRASONIC_ECHO, HIGH, 30000);
  float dist = dur * 0.034 / 2.0;
  if (dur == 0)             dist = BIN_HEIGHT_CM;
  if (dist > BIN_HEIGHT_CM) dist = BIN_HEIGHT_CM;
  if (dist < 2)             dist = 2;
  return constrain(map((int)dist, BIN_HEIGHT_CM, 2, 0, 100), 0, 100);
}


//  STARTUP TEST

void runStartupTest() {
  rgbOff();
  digitalWrite(BUZZER_PIN, LOW);
  lcd.clear();
  lcd.setCursor(0, 0); lcd.print(F("CLEANING GOTHAM!"));
  lcd.setCursor(0, 1); lcd.print(F("Testing...      "));
  smartDelay(800);

  lcd.setCursor(0, 1); lcd.print(F("RGB: RED        "));
  showRed();    smartDelay(400); rgbOff();
  lcd.setCursor(0, 1); lcd.print(F("RGB: GREEN      "));
  showGreen();  smartDelay(400); rgbOff();
  lcd.setCursor(0, 1); lcd.print(F("RGB: BLUE       "));
  showBlue();   smartDelay(400); rgbOff();
  lcd.setCursor(0, 1); lcd.print(F("RGB: YELLOW     "));
  showYellow(); smartDelay(400); rgbOff();

  lcd.setCursor(0, 1); lcd.print(F("BUZZER: BEEP!   "));
  digitalWrite(BUZZER_PIN, HIGH); smartDelay(300);
  digitalWrite(BUZZER_PIN, LOW);

  lcd.clear();
  lcd.setCursor(0, 0); lcd.print(F("All Tests PASS!"));
  lcd.setCursor(0, 1); lcd.print(F("System ready!   "));
  smartDelay(1200);
}
