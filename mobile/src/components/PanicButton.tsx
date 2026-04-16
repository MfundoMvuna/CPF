import React, { useState, useCallback } from 'react';
import {
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
  Alert,
  ActivityIndicator,
  Vibration,
} from 'react-native';
import * as Location from 'expo-location';
import { triggerPanic } from '../services/panicService';

export default function PanicButton() {
  const [isSending, setIsSending] = useState(false);
  const [sent, setSent] = useState(false);

  const handlePanic = useCallback(async () => {
    try {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        Alert.alert('Permission required', 'Location access is needed to send your GPS coordinates with the panic alert.');
        return;
      }

      setIsSending(true);
      Vibration.vibrate(500);

      const location = await Location.getCurrentPositionAsync({
        accuracy: Location.Accuracy.High,
      });

      await triggerPanic({
        latitude: location.coords.latitude,
        longitude: location.coords.longitude,
        description: 'Emergency panic alert triggered from mobile app',
      });

      setSent(true);
      Vibration.vibrate([0, 200, 100, 200]);
      Alert.alert('Alert Sent', 'Your panic alert has been sent with your GPS location. Help is on the way.', [
        { text: 'OK', onPress: () => setSent(false) },
      ]);
    } catch (e: any) {
      Alert.alert('Error', e.message || 'Failed to send panic alert. Please try again.');
    } finally {
      setIsSending(false);
    }
  }, []);

  return (
    <View style={styles.container}>
      <TouchableOpacity
        style={[
          styles.button,
          isSending && styles.buttonSending,
          sent && styles.buttonSent,
        ]}
        onPress={handlePanic}
        disabled={isSending}
        activeOpacity={0.7}
      >
        {isSending ? (
          <>
            <ActivityIndicator color="#fff" size="large" />
            <Text style={styles.buttonText}>Sending...</Text>
          </>
        ) : sent ? (
          <>
            <Text style={styles.checkmark}>✓</Text>
            <Text style={styles.buttonText}>Alert Sent</Text>
          </>
        ) : (
          <>
            <Text style={styles.icon}>🚨</Text>
            <Text style={styles.buttonText}>PANIC</Text>
            <Text style={styles.subText}>Tap to send emergency alert</Text>
          </>
        )}
      </TouchableOpacity>
    </View>
  );
}

const BUTTON_SIZE = 220;

const styles = StyleSheet.create({
  container: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  button: {
    width: BUTTON_SIZE,
    height: BUTTON_SIZE,
    borderRadius: BUTTON_SIZE / 2,
    backgroundColor: '#e94560',
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 10,
    shadowColor: '#e94560',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.5,
    shadowRadius: 12,
    borderWidth: 4,
    borderColor: '#ff6b81',
  },
  buttonSending: {
    backgroundColor: '#c0392b',
    borderColor: '#e74c3c',
  },
  buttonSent: {
    backgroundColor: '#27ae60',
    borderColor: '#2ecc71',
  },
  icon: {
    fontSize: 48,
    marginBottom: 8,
  },
  checkmark: {
    fontSize: 48,
    color: '#fff',
    marginBottom: 8,
  },
  buttonText: {
    color: '#fff',
    fontSize: 28,
    fontWeight: 'bold',
  },
  subText: {
    color: 'rgba(255,255,255,0.8)',
    fontSize: 12,
    marginTop: 4,
  },
});
