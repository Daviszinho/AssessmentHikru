export const POSITION_CONSTANTS = {
  VALIDATION: {
    MAX_TITLE_LENGTH: 100,
    MAX_DESCRIPTION_LENGTH: 1000,
    MIN_BUDGET: 0,
    MIN_RECRUITER_ID: 1,
    MIN_DEPARTMENT_ID: 1
  },
  STATUS: {
    DRAFT: 'draft',
    OPEN: 'open',
    CLOSED: 'closed',
    ARCHIVED: 'archived'
  },
  STYLES: {
    CONTAINER: {
      backgroundColor: '#f0f0f0',
      padding: '10px',
      marginBottom: '15px',
      borderRadius: '4px',
      border: '1px solid #ddd'
    },
    BUTTON: {
      backgroundColor: '#4CAF50',
      color: 'white',
      border: 'none',
      padding: '5px 10px',
      margin: '0 5px',
      borderRadius: '4px',
      cursor: 'pointer'
    },
    DELETE_BUTTON: {
      backgroundColor: '#f44336',
      color: 'white',
      border: 'none',
      padding: '5px 10px',
      margin: '0 5px',
      borderRadius: '4px',
      cursor: 'pointer'
    },
    CANCEL_BUTTON: {
      backgroundColor: 'transparent',
      border: '1px solid #f44336',
      color: '#f44336',
      padding: '4px 12px',
      borderRadius: '4px',
      cursor: 'pointer'
    },
    FORM_CONTAINER: {
      background: '#f5f5f5',
      padding: '16px',
      borderRadius: '8px',
      marginTop: '8px',
      color: '#333'
    },
    ERROR_MESSAGE: {
      color: 'red',
      marginTop: '8px'
    }
  }
} as const;

export type Status = typeof POSITION_CONSTANTS.STATUS[keyof typeof POSITION_CONSTANTS.STATUS]; 