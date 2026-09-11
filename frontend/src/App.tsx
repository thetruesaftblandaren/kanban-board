import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import BoardsPage from "./pages/BoardsPage";
import BoardDetailPage from "./pages/BoardDetailPage";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/boards" element={<BoardsPage />} />
        <Route path="/boards/:boardId" element={<BoardDetailPage />} />
        <Route path="/" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App
