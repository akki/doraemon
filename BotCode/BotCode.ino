#define M1F 10
#define M1B 11
#define M2F 12
#define M2B 13

char data;
void setup()
{
  // put your setup code here, to run once:
  pinMode(10 , OUTPUT);
  pinMode(11 , OUTPUT);
  pinMode(12 , OUTPUT);
  pinMode(13 , OUTPUT);
  Serial.begin(9600);
}

void loop()
{
  // put your main code here, to run repeatedly:
 if(Serial.available() > 0){
   data = Serial.read();
   
   switch(data){
     
     case 'F' :
       digitalWrite(M1F,HIGH);
       digitalWrite(M1B,LOW);
       digitalWrite(M2F,HIGH);
       digitalWrite(M2B,LOW);
    case 'B' :
      digitalWrite(M1F,LOW);
      digitalWrite(M1B,HIGH);
      digitalWrite(M2F,LOW);
      digitalWrite(M2B,HIGH);
    case 'L' :
      digitalWrite(M1F,LOW);
      digitalWrite(M1B,LOW);
      digitalWrite(M2F,HIGH);
      digitalWrite(M2B,LOW);
    case 'R' :
      digitalWrite(M1F,HIGH);
      digitalWrite(M1B,LOW);
      digitalWrite(M2F,LOW);
      digitalWrite(M2B,LOW);
    case 'S' :
       digitalWrite(M1F,LOW);
       digitalWrite(M1B,LOW);
       digitalWrite(M2F,LOW);
       digitalWrite(M2B,LOW);
     default :
       digitalWrite(M1F,LOW);
       digitalWrite(M1B,LOW);
       digitalWrite(M2F,LOW);
       digitalWrite(M2B,LOW);  
   }
 } 
}
