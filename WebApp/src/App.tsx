import React, { useState } from 'react';
import PositionTable from './components/PositionsTable.tsx';
import type { Position } from './components/PositionsTable.tsx';
import reactLogo from './assets/react.svg';
import viteLogo from '/vite.svg';
import './App.css';

const initialPositions = [
  {
    title: 'Frontend Developer',
    description: 'Develop UI components',
    location: 'Remote',
    status: 'open',
    recruiterId: 'R001',
    departmentId: 'D001',
    budget: 50000,
    closingDate: '2025-07-01',
  },
  {
    title: 'Backend Developer',
    description: 'Work on APIs',
    location: 'Onsite',
    status: 'draft',
    recruiterId: 'R002',
    departmentId: 'D002',
    budget: 60000,
  },
];

function App() {
  const [positions, setPositions] = useState(initialPositions);
  const [expandedIndex, setExpandedIndex] = useState<number | null>(null);

  const handleDelete = (idx: number) => {
    setPositions(positions => positions.filter((_, i) => i !== idx));
    if (expandedIndex === idx) setExpandedIndex(null);
    else if (expandedIndex !== null && expandedIndex > idx) setExpandedIndex(expandedIndex - 1);
  };

  const handleUpdate = (idx: number) => {
    alert(`Update position: ${positions[idx].title}`);
  };

  const handleAdd = (newPosition: Position) => {
    setPositions(prev => [...prev, newPosition]);
  };

  const handleExpand = (idx: number) => {
    setExpandedIndex(expandedIndex === idx ? null : idx);
  };

  return (
    <div>
      <div>
        <h1>Welcome to Hikrutech - Full Stack Lead</h1>
        <h2>Full Stack Lead - Davis Penaranda</h2>
        <a href="https://www.hikrutech.com/" target="_blank">
          <img src="https://cdn.prod.website-files.com/66c2cf5006b8d37eca26c0d2/66d030e05d6df2a0e0b5aabe_logo-footer.png" className="logo" alt="Vite logo" />
        </a>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <PositionTable
        positions={positions}
        expandedIndex={expandedIndex}
        onExpand={handleExpand}
        onDelete={handleDelete}
        onUpdate={handleUpdate}
        onAdd={handleAdd}
      />
    </div>
  );
}

export default App;
