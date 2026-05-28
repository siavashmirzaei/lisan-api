import { useAuth } from '@clerk/clerk-expo';
import { Redirect, Stack } from 'expo-router';

export default function AppLayout() {
  const { isSignedIn, isLoaded } = useAuth();

  if (isLoaded && !isSignedIn) {
    return <Redirect href="/sign-in" />;
  }

  return <Stack screenOptions={{ headerShown: false }} />;
}
