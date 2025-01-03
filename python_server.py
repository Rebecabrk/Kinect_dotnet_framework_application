import cv2
import mediapipe as mp
import socket
import json
import numpy as np

# Initialize MediaPipe Hands
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(static_image_mode=False, max_num_hands=2, min_detection_confidence=0.5)
mp_drawing = mp.solutions.drawing_utils

# Set up the server
HOST = '127.0.0.1'  # Localhost
PORT = 12345        # Port for communication

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.bind((HOST, PORT))
server_socket.listen(1)
print("Server listening on:", (HOST, PORT))

# Accept connection
conn, addr = server_socket.accept()
print("Connected by:", addr)

# Start processing Kinect feed
while True:
    # Receive frame from Kinect via WPF app
    data = conn.recv(1024*1024)  # Adjust buffer size as needed
    if not data:
        break

    # Convert byte data to numpy array
    frame = cv2.imdecode(np.frombuffer(data, np.uint8), cv2.IMREAD_COLOR)

    # Process with MediaPipe
    rgb_frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    results = hands.process(rgb_frame)

    # Extract landmarks
    landmarks_list = []
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            for lm in hand_landmarks.landmark:
                landmarks_list.append({'x': lm.x, 'y': lm.y, 'z': lm.z})

    # Send landmarks back to WPF app
    conn.sendall(json.dumps(landmarks_list).encode('utf-8'))

    # Optional: Display the frame with landmarks (for debugging)
    if results.multi_hand_landmarks:
        for hand_landmarks in results.multi_hand_landmarks:
            mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS)

    cv2.imshow("Hand Detection", frame)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        break

# Cleanup
hands.close()
conn.close()
server_socket.close()
cv2.destroyAllWindows()
