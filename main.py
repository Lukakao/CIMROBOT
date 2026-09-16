import serial
import time

# Debe coincidir EXACTO con SYNC_LOW / SYNC_HIGH del Arduino
SYNC_LOW = 0x55
SYNC_HIGH = 0xAA


def decimal_to_bytes(decimal, byte_length=2):
    return decimal.to_bytes(byte_length, byteorder='little')


if __name__ == "__main__":
    PORT = "/COM5"
    BAUDRATE = 115200

    robot = serial.Serial(PORT, BAUDRATE, timeout=1)
    count = 55
    time.sleep(0.1)
    while True:
        # Send command
        servo1 = decimal_to_bytes(500)
        servo2 = decimal_to_bytes(250)
        servo3 = decimal_to_bytes(330)
        servo4 = decimal_to_bytes(300)
        servo5 = decimal_to_bytes(count)
        servo6 = decimal_to_bytes(200)

        # El 7mo par ya no transporta un servo: es la marca de sincronizacion
        sync = bytes([SYNC_LOW, SYNC_HIGH])

        data = servo1 + servo2 + servo3 + servo4 + servo5 + servo6 + sync
        robot.write(data)
        count += 1
        time.sleep(0.1)

        # Receive response (eco de 14 bytes, con espera activa en vez de in_waiting)
        response = robot.read(14)

        if len(response) < 14:
            print(f"{count} Eco incompleto ({len(response)} bytes): {response}")
        elif response[12] != SYNC_LOW or response[13] != SYNC_HIGH:
            print(f"{count} Eco sin marca de sync valida -> DESINCRONIZADO: {response}")
            robot.reset_input_buffer()
        elif response[:12] != data[:12]:
            print(f"{count} Eco no coincide con lo enviado: {response}")
        else:
            print(f"{count} OK - Received: {response}")