import React from 'react';
import { usePositions } from './hooks/usePositions';
import { useFocusManagement } from './hooks/useFocusManagement';
import PositionTable from './components/PositionsTable';
import Header from './components/Header';
import Footer from './components/Footer';
import Loading from './components/Loading';
import ErrorMessage from './components/ErrorMessage';
import { API_URL } from './config/api';
import './App.css';

const App: React.FC = () => {
  // Use custom hooks for state and side effects
  const { 
    positions, 
    isLoading, 
    error, 
    expandedIndex, 
    addPosition, 
    deletePosition, 
    updatePosition, 
    toggleExpand 
  } = usePositions();
  
  // Focus management
  const { mainContentRef, skipToContentRef, handleSkipToContent } = useFocusManagement();

  return (
    <div className="app-container" role="application">
      {/* Skip to main content link - hidden until focused */}
      <a
        href="#main-content"
        ref={skipToContentRef}
        className="skip-to-content"
        onKeyDown={handleSkipToContent}
      >
        Skip to main content
      </a>

      <Header />

      <main 
        id="main-content"
        ref={mainContentRef}
        role="main"
        tabIndex={-1}
        aria-labelledby="page-title"
        className={isLoading ? 'loading' : ''}
      >
        <ErrorMessage error={error} />
        
        <Loading 
          isLoading={isLoading} 
          message="Loading positions..." 
        />
        
        {!isLoading && (
          <PositionTable 
            positions={positions}
            expandedIndex={expandedIndex}
            onExpand={toggleExpand}
            onDelete={deletePosition}
            onUpdate={updatePosition}
            onAdd={addPosition}
          />
        )}
      </main>
      
      <Footer apiUrl={API_URL} />
    </div>
  );
}

export default App;
