import React from 'react';
import { View, Text, StyleSheet, SafeAreaView } from 'react-native';
import PanicButton from '../components/PanicButton';
import { useAuthStore } from '../stores/authStore';

export default function HomeScreen() {
  const user = useAuthStore((s) => s.user);

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.greeting}>Hello, {user?.fullName?.split(' ')[0] ?? 'Member'}</Text>
        <Text style={styles.headerSub}>CPF Community Safety</Text>
      </View>

      <View style={styles.panicContainer}>
        <PanicButton />
      </View>

      <View style={styles.infoSection}>
        <Text style={styles.infoTitle}>How it works</Text>
        <Text style={styles.infoText}>
          Press the panic button to immediately send an emergency alert with
          your GPS coordinates to all nearby CPF members and patrol units.
        </Text>
      </View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#1a1a2e',
  },
  header: {
    paddingHorizontal: 24,
    paddingTop: 16,
    paddingBottom: 8,
  },
  greeting: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#fff',
  },
  headerSub: {
    fontSize: 14,
    color: '#888',
    marginTop: 2,
  },
  panicContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  infoSection: {
    paddingHorizontal: 32,
    paddingBottom: 32,
  },
  infoTitle: {
    fontSize: 16,
    fontWeight: '600',
    color: '#ccc',
    marginBottom: 6,
  },
  infoText: {
    fontSize: 14,
    color: '#888',
    lineHeight: 20,
  },
});
