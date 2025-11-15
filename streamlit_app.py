import streamlit as st

st.set_page_config(page_title="ECN AI Test", page_icon="🤖")

st.title("ECN Manager AI – Demo")

# Ô nhập câu hỏi
user_input = st.text_area("Nhập câu hỏi về ECN / WI / TNA:")

if st.button("Hỏi AI"):
    if not user_input.strip():
        st.warning("Anh hãy nhập câu hỏi trước.")
    else:
        # Tạm thời trả lời demo cho đỡ lỗi, sau này nối với AI thật
        st.write("**Câu hỏi:**", user_input)
        st.write("**Trả lời demo:** Đây là bản thử nghiệm. Sau sẽ kết nối với AI ECN Manager thực tế.")
