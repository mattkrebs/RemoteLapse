/*
 * Remote Lapse Bluetooth Controlled Time Lapse Controller
 */
 
 //Incoming Format
 // *START|PULSE|INTERVAL|FRAMES|DIRECTION#
#define START_CMD_CHAR '*'
#define END_CMD_CHAR '#'
#define DIV_CMD_CHAR '|'


int shutterPin = 8;
int focusPin = 6;
int ledPin = 7;
int motorPin = 11;              
int relay = 12;              

int start = 0; // on = 1; off = 0;
int frames = 150;
int interval = 2000; //milliseconds
int pulse = 100; //milliseconds 
int direct = 0; //0 = right; 1 = left



String inText;


void setup()                    // run once, when the sketch starts
{
  Serial1.begin(9600); 
while (!Serial1) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }  // set up Serial library at 9600 bps
  pinMode(shutterPin, OUTPUT); 
  pinMode(motorPin, OUTPUT);
  pinMode(relay, OUTPUT);
    pinMode(ledPin, OUTPUT);
}


void motorLeft() {
 
  digitalWrite(motorPin, HIGH);
  digitalWrite(relay, LOW);
  digitalWrite(ledPin, HIGH);
  delay(pulse);  
  digitalWrite(ledPin, LOW);  
  digitalWrite(motorPin, LOW);
  digitalWrite(relay, HIGH);

  Serial.println("Move Left: ");
  Serial.println(interval);
}

void motorRight() {
 
  digitalWrite(motorPin, LOW);
  digitalWrite(relay, HIGH);
  digitalWrite(ledPin, HIGH);
  delay(pulse);  
  digitalWrite(ledPin, LOW);
  digitalWrite(motorPin, HIGH);
  digitalWrite(relay, LOW);
  
  Serial.println("Move Rig: ");
  Serial.println(interval);
}

void loop()                     // run over and over again
{
  Serial1.flush();
  int ard_command = 0;
  int pin_num = 0;
  int pin_value = 0;
  int photos_taken = 0;
  char get_char = ' ';
  
  if (Serial1.available() < 1) return;
  
  //parse incoming start command
  get_char = Serial1.read(); // read first character

 
  
  if (get_char != START_CMD_CHAR) return; 
  
  
  
   // parse incoming command type
  start = Serial1.parseInt(); // read the command  
  pulse = Serial1.parseInt(); // read the command
  interval = Serial1.parseInt(); // read the interval
  frames = Serial1.parseInt();  // read the frames
  direct = Serial1.parseInt(); // read the direction
  
  Serial.print(start);
  Serial.println("pulse");
  Serial.print(pulse);
    Serial.println("interval");
  Serial.print(interval);
    Serial.println("frames");
  Serial.print(frames);
    Serial.println("direction");
  Serial.print(direct);

  
  
  while(start == 1){
    if (direct == 1){ //Left
      //Take picture
      digitalWrite(shutterPin, HIGH);
      delay(500);
      digitalWrite(shutterPin, LOW);
      motorLeft(); 
      delay(interval);     
      //Move
      //Pause    
    } 
    if (direct == 0){
      //Take picture
      digitalWrite(shutterPin, HIGH);
      delay(500);
      digitalWrite(shutterPin, LOW);
      motorLeft();
         delay(interval);
    } 
    
    if (photos_taken < frames)  {
      photos_taken++;
      Serial.print("Taken :");
     Serial.print(photos_taken);
     Serial.print(" of ");
     Serial.print(frames);
      Serial.println(" ");
    }else{
      start = 0;
    }
  }
  
}
