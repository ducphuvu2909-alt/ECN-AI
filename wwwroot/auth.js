const auth = (function(){
  const KEY = 'ecn_jwt';
  function token(){ return localStorage.getItem(KEY); }
  function setToken(t){ if(t) localStorage.setItem(KEY, t); }
  function clear(){ localStorage.removeItem(KEY); }

  async function login(username, password, dept){
    const r = await fetch('api/login', {
      method:'POST',
      headers:{'Content-Type':'application/json'},
      body: JSON.stringify({ username, password, dept })
    });
    if(!r.ok){
      try { const t = await r.text(); throw new Error(t.replace(/<[^>]+>/g,'').slice(0,180)); }
      catch(e){ throw new Error('HTTP ' + r.status); }
    }
    const data = await r.json();
    setToken(data.token);
    return { token: data.token, user: data.user };
  }

  async function me(){
    const t = token();
    if(!t) return null;
    const r = await fetch('api/me', { headers:{'Authorization':'Bearer ' + t} });
    if(!r.ok) return null;
    return await r.json();
  }

  function logout(){ clear(); }
  return { login, me, logout, token };
})();