import serial
import time
from typing import Optional
     
     
def decimal_to_bytes(decimal, byte_length=2):
    return decimal.to_bytes(byte_length, byteorder='little')


if __name__ == "__main__":
    robot = serial.Serial(port='/dev/ttyACM0', baudrate=115200)
    count = 55
    time.sleep(1)
    while True:
            # Send command
            
            servo1 = decimal_to_bytes(0)
            servo2 = decimal_to_bytes(300)
            servo3 = decimal_to_bytes(300)
            servo4 = decimal_to_bytes(0)
            servo5 = decimal_to_bytes(0)
            servo6 = decimal_to_bytes(count)
            servo7 = decimal_to_bytes(0)
            
            data = servo1 + servo2 + servo3 + servo4 + servo5 + servo6 + servo7
            robot.write(data) 
            count += 1
            time.sleep(0.5)
            # Receive response
            response = robot.read(robot.in_waiting)
            print(f"{count} Received: {response}")
        
