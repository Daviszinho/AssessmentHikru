import React from 'react';

interface LoadingProps {
  isLoading: boolean;
  message?: string;
}

const Loading: React.FC<LoadingProps> = ({ 
  isLoading, 
  message = 'Loading...' 
}) => {
  if (!isLoading) return null;

  return (
    <div 
      role="status" 
      aria-live="polite"
      aria-busy={isLoading}
    >
      <p>{message}</p>
      <div className="loading-spinner" aria-hidden="true"></div>
    </div>
  );
};

export default Loading;
