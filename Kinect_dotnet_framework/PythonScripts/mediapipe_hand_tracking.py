import mediapipe as mp
import cv2
import numpy as np

mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.5, min_tracking_confidence=0.5)


def process_frame(pixels, width, height):
    # Convert Kinect pixel data to NumPy array
    frame = np.frombuffer(pixels, dtype=np.uint8).reshape((height, width, 4))
    frame = cv2.cvtColor(frame, cv2.COLOR_BGRA2RGB)

    # Process with MediaPipe
    results = hands.process(frame)
    if not results.multi_hand_landmarks:
        return "No hands detected."

    # Extract landmarks
    landmark_strs = []
    for hand_landmarks in results.multi_hand_landmarks:
        for landmark in hand_landmarks.landmark:
            landmark_strs.append(f"({landmark.x:.2f}, {landmark.y:.2f}, {landmark.z:.2f})")

    return "\n".join(landmark_strs)
