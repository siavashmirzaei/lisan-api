import { ClerkProvider, useAuth } from '@clerk/clerk-expo';
import { tokenCache } from '../lib/token-cache';
import { Sentry } from '../lib/sentry';
import { registerTokenGetter } from '../lib/api';
import { Stack } from 'expo-router';
import { useEffect } from 'react';

function TokenBridge() {
  const { getToken } = useAuth();
  useEffect(() => {
    registerTokenGetter(() => getToken());
  }, [getToken]);
  return null;
}

function RootLayout() {
  return (
    <ClerkProvider
      publishableKey={process.env.EXPO_PUBLIC_CLERK_PUBLISHABLE_KEY!}
      tokenCache={tokenCache}
    >
      <TokenBridge />
      <Sentry.ErrorBoundary>
        <Stack screenOptions={{ headerShown: false }} />
      </Sentry.ErrorBoundary>
    </ClerkProvider>
  );
}

export default Sentry.wrap(RootLayout);
