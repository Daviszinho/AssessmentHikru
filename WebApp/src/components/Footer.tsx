import React from 'react';

interface FooterProps {
  apiUrl: string;
}

const Footer: React.FC<FooterProps> = ({ apiUrl }) => (
  <footer className="app-footer" role="contentinfo">
    <div className="footer-content">
      <p>
        API Endpoint:{' '}
        <a 
          href={apiUrl} 
          target="_blank" 
          rel="noopener noreferrer"
          className="api-link"
        >
          {apiUrl}
        </a>
      </p>
      <p>
        <a 
          href="https://github.com/daviszinho/AssessmentHikru" 
          target="_blank" 
          rel="noopener noreferrer"
          className="github-link"
        >
          <span className="github-icon" aria-hidden="true">📄</span> View on GitHub
        </a>
      </p>
    </div>
  </footer>
);

export default Footer;
