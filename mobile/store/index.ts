import { create } from 'zustand';

interface AppState {
  isAuthenticated: boolean;
  childProfileId: string | null;
  activeSessionId: string | null;
  isRecording: boolean;
  isPlayingResponse: boolean;
}

const useAppStore = create<AppState>()(() => ({
  isAuthenticated: false,
  childProfileId: null,
  activeSessionId: null,
  isRecording: false,
  isPlayingResponse: false,
}));

export default useAppStore;
