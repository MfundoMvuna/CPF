import React, { useState } from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ActivityIndicator,
  ScrollView,
} from 'react-native';
import { useAuthStore } from '../stores/authStore';

interface Props {
  onSwitchToLogin: () => void;
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_REGEX = /^\+?[0-9]{10,15}$/;

export default function RegisterScreen({ onSwitchToLogin }: Props) {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const { register, isLoading, error, clearError } = useAuthStore();

  const mark = (field: string) => setTouched((t) => ({ ...t, [field]: true }));

  const nameError = touched.fullName && !fullName.trim() ? 'Full name is required' : '';
  const emailError = touched.email && !email.trim()
    ? 'Email is required'
    : touched.email && !EMAIL_REGEX.test(email.trim())
    ? 'Enter a valid email address'
    : '';
  const phoneError = touched.phone && !phone.trim()
    ? 'Phone number is required'
    : touched.phone && !PHONE_REGEX.test(phone.trim().replace(/[\s-]/g, ''))
    ? 'Enter a valid phone number (e.g. +27821234567)'
    : '';
  const passwordError = touched.password && !password
    ? 'Password is required'
    : touched.password && password.length < 8
    ? 'Password must be at least 8 characters'
    : '';
  const confirmError = touched.confirmPassword && password !== confirmPassword
    ? 'Passwords do not match'
    : '';

  const hasErrors = !fullName.trim() || !EMAIL_REGEX.test(email.trim())
    || !PHONE_REGEX.test(phone.trim().replace(/[\s-]/g, '')) || password.length < 8
    || password !== confirmPassword;

  const handleRegister = async () => {
    const allTouched = { fullName: true, email: true, phone: true, password: true, confirmPassword: true };
    setTouched(allTouched);
    if (hasErrors) return;
    clearError();
    await register({
      fullName: fullName.trim(),
      email: email.trim(),
      password,
      phoneNumber: phone.trim(),
    });
  };

  return (
    <KeyboardAvoidingView
      style={styles.container}
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
    >
      <ScrollView contentContainerStyle={styles.inner} keyboardShouldPersistTaps="handled">
        <Text style={styles.logo}>CPF</Text>
        <Text style={styles.subtitle}>Create your account</Text>

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <TextInput
          style={[styles.input, nameError ? styles.inputError : null]}
          placeholder="Full Name"
          placeholderTextColor="#888"
          value={fullName}
          onChangeText={setFullName}
          onBlur={() => mark('fullName')}
        />
        {nameError ? <Text style={styles.fieldError}>{nameError}</Text> : null}

        <TextInput
          style={[styles.input, emailError ? styles.inputError : null]}
          placeholder="Email"
          placeholderTextColor="#888"
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
          onBlur={() => mark('email')}
        />
        {emailError ? <Text style={styles.fieldError}>{emailError}</Text> : null}

        <TextInput
          style={[styles.input, phoneError ? styles.inputError : null]}
          placeholder="Phone Number (e.g. +27821234567)"
          placeholderTextColor="#888"
          keyboardType="phone-pad"
          value={phone}
          onChangeText={setPhone}
          onBlur={() => mark('phone')}
        />
        {phoneError ? <Text style={styles.fieldError}>{phoneError}</Text> : null}

        <View style={styles.passwordContainer}>
          <TextInput
            style={[styles.passwordInput, passwordError ? styles.inputError : null]}
            placeholder="Password"
            placeholderTextColor="#888"
            secureTextEntry={!showPassword}
            value={password}
            onChangeText={setPassword}
            onBlur={() => mark('password')}
          />
          <TouchableOpacity
            style={styles.eyeButton}
            onPress={() => setShowPassword(!showPassword)}
          >
            <Text style={styles.eyeIcon}>{showPassword ? '🙈' : '👁'}</Text>
          </TouchableOpacity>
        </View>
        {passwordError ? <Text style={styles.fieldError}>{passwordError}</Text> : null}

        <View style={styles.passwordContainer}>
          <TextInput
            style={[styles.passwordInput, confirmError ? styles.inputError : null]}
            placeholder="Confirm Password"
            placeholderTextColor="#888"
            secureTextEntry={!showConfirm}
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            onBlur={() => mark('confirmPassword')}
          />
          <TouchableOpacity
            style={styles.eyeButton}
            onPress={() => setShowConfirm(!showConfirm)}
          >
            <Text style={styles.eyeIcon}>{showConfirm ? '🙈' : '👁'}</Text>
          </TouchableOpacity>
        </View>
        {confirmError ? <Text style={styles.fieldError}>{confirmError}</Text> : null}

        <TouchableOpacity
          style={[styles.button, isLoading && styles.buttonDisabled]}
          onPress={handleRegister}
          disabled={isLoading}
        >
          {isLoading ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <Text style={styles.buttonText}>Register</Text>
          )}
        </TouchableOpacity>

        <TouchableOpacity onPress={onSwitchToLogin} style={styles.link}>
          <Text style={styles.linkText}>
            Already have an account? <Text style={styles.linkBold}>Sign In</Text>
          </Text>
        </TouchableOpacity>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#1a1a2e',
  },
  inner: {
    flexGrow: 1,
    justifyContent: 'center',
    paddingHorizontal: 32,
    paddingVertical: 40,
  },
  logo: {
    fontSize: 48,
    fontWeight: 'bold',
    color: '#e94560',
    textAlign: 'center',
    marginBottom: 4,
  },
  subtitle: {
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
    marginBottom: 32,
  },
  error: {
    color: '#e94560',
    textAlign: 'center',
    marginBottom: 12,
    fontSize: 14,
  },
  input: {
    backgroundColor: '#16213e',
    borderRadius: 12,
    paddingHorizontal: 16,
    paddingVertical: 14,
    color: '#fff',
    fontSize: 16,
    marginBottom: 14,
    borderWidth: 1,
    borderColor: '#0f3460',
  },
  inputError: {
    borderColor: '#e94560',
  },
  fieldError: {
    color: '#e94560',
    fontSize: 12,
    marginTop: -10,
    marginBottom: 10,
    marginLeft: 4,
  },
  passwordContainer: {
    flexDirection: 'row' as const,
    alignItems: 'center' as const,
    backgroundColor: '#16213e',
    borderRadius: 12,
    borderWidth: 1,
    borderColor: '#0f3460',
    marginBottom: 14,
  },
  passwordInput: {
    flex: 1,
    paddingHorizontal: 16,
    paddingVertical: 14,
    color: '#fff',
    fontSize: 16,
    borderRadius: 12,
  },
  eyeButton: {
    paddingHorizontal: 14,
    paddingVertical: 14,
  },
  eyeIcon: {
    fontSize: 20,
  },
  button: {
    backgroundColor: '#e94560',
    borderRadius: 12,
    paddingVertical: 16,
    alignItems: 'center',
    marginTop: 8,
  },
  buttonDisabled: {
    opacity: 0.7,
  },
  buttonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: '600',
  },
  link: {
    marginTop: 24,
    alignItems: 'center',
  },
  linkText: {
    color: '#ccc',
    fontSize: 14,
  },
  linkBold: {
    color: '#e94560',
    fontWeight: '600',
  },
});
