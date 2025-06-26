import { useRef, useEffect } from 'react';

export const useFocusManagement = () => {
  const mainContentRef = useRef<HTMLElement>(null);
  const skipToContentRef = useRef<HTMLAnchorElement>(null);
  const lastFocusedElement = useRef<HTMLElement | null>(null);

  const handleSkipToContent = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && mainContentRef.current) {
      e.preventDefault();
      mainContentRef.current.focus();
    }
  };

  useEffect(() => {
    // Save the currently focused element when the component mounts
    lastFocusedElement.current = document.activeElement as HTMLElement;
    
    // Set focus to the main content area when the component mounts
    if (mainContentRef.current) {
      mainContentRef.current.focus();
    }

    // Cleanup function to restore focus when component unmounts
    return () => {
      if (lastFocusedElement.current) {
        lastFocusedElement.current.focus();
      }
    };
  }, []);

  return {
    mainContentRef,
    skipToContentRef,
    handleSkipToContent,
  };
};
