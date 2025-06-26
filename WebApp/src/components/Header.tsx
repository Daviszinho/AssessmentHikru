import React from 'react';
import viteLogo from '../assets/react.svg';
import reactLogo from '/vite.svg';

const Header: React.FC = () => (
  <header className="app-header" role="banner">
    <h1>Welcome to Hikrutech</h1>
    <h2>Full Stack Lead - Davis Peñaranda</h2>
    <nav aria-label="External links">
      <ul className="logo-list">
        <li>
          <a 
            href="https://www.hikrutech.com/" 
            target="_blank" 
            rel="noopener noreferrer"
            aria-label="Visit Hikrutech website"
          >
            <img 
              src="https://cdn.prod.website-files.com/66c2cf5006b8d37eca26c0d2/66d030e05d6df2a0e0b5aabe_logo-footer.png" 
              className="logo" 
              alt="Hikrutech logo" 
            />
          </a>
        </li>
        <li>
          <a 
            href="https://vite.dev" 
            target="_blank" 
            rel="noopener noreferrer"
            aria-label="Learn about Vite"
          >
            <img 
              src={viteLogo} 
              className="logo" 
              alt="Vite logo" 
            />
          </a>
        </li>
        <li>
          <a 
            href="https://react.dev" 
            target="_blank" 
            rel="noopener noreferrer"
            aria-label="Learn about React"
          >
            <img 
              src={reactLogo} 
              className="logo react" 
              alt="React logo" 
            />
          </a>
        </li>
      </ul>
    </nav>
  </header>
);

export default Header;
