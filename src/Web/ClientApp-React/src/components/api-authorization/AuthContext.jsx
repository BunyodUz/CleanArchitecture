import { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext(null);

const ANONYMOUS = { isAuthenticated: false, userName: null, roles: [], permissions: [] };

export function AuthProvider({ children }) {
  const [user, setUser] = useState(ANONYMOUS);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    fetch('/account/user', { credentials: 'include' })
      .then(response => response.ok ? response.json() : ANONYMOUS)
      .then(setUser)
      .catch(() => setUser(ANONYMOUS))
      .finally(() => setIsLoading(false));
  }, []);

  // Keycloak's hosted pages are the login/registration UI — these are redirects, not API calls.
  const login = (returnUrl = '/') => {
    window.location.href = `/account/login?returnUrl=${encodeURIComponent(returnUrl)}`;
  };

  const logout = () => {
    window.location.href = `/account/logout?returnUrl=${encodeURIComponent('/')}`;
  };

  return (
    <AuthContext.Provider value={{ ...user, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
