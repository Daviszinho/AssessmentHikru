import React from 'react';

interface ErrorMessageProps {
  error: string | null;
  className?: string;
}

const ErrorMessage: React.FC<ErrorMessageProps> = ({ 
  error, 
  className = '' 
}) => {
  if (!error) return null;

  return (
    <div 
      role="alert" 
      aria-live="assertive"
      className={`error-message ${className}`}
    >
      {error}
    </div>
  );
};

export default ErrorMessage;
